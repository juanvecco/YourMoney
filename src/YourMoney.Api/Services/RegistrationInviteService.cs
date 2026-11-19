using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourMoney.Infrastructure.Data;

namespace YourMoney.Api.Services;

public sealed record InviteIssued(string Codigo, string Link, DateTimeOffset ExpiraEm);
public sealed record InviteCheck(bool Valido, string Motivo);
public sealed record InviteSummary(string Id, DateTimeOffset ExpiraEm, DateTimeOffset? UsadoEm, string UsadoPorEmail, bool Revogado);

public sealed class RegistrationInviteService
{
    private const string Provider = "YourMoneyInvite";
    private readonly ApplicationDbContext _db;
    private readonly TimeProvider _clock;

    public RegistrationInviteService(ApplicationDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<InviteIssued> IssueAsync(string adminId, string frontendBaseUrl, CancellationToken cancellationToken = default)
    {
        var code = adminId + "." + Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var expires = _clock.GetUtcNow().AddDays(7);
        _db.UserTokens.Add(new IdentityUserToken<string>
        {
            UserId = adminId,
            LoginProvider = Provider,
            Name = Hash(code),
            Value = expires.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)
        });
        await _db.SaveChangesAsync(cancellationToken);
        return new InviteIssued(code, frontendBaseUrl.TrimEnd('/') + "/cadastro?convite=" + Uri.EscapeDataString(code), expires);
    }

    public async Task<InviteCheck> CheckAsync(string code, CancellationToken cancellationToken = default)
    {
        if (!TryKey(code, out var adminId, out var hash))
            return new InviteCheck(false, "Este convite não é válido.");
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider && token.Name == hash)
            .Select(token => token.Value)
            .SingleOrDefaultAsync(cancellationToken);
        if (value == null) return new InviteCheck(false, "Este convite não é válido.");
        if (value.StartsWith("U:", StringComparison.Ordinal)) return new InviteCheck(false, "Este convite já foi usado.");
        if (value.StartsWith("R:", StringComparison.Ordinal)) return new InviteCheck(false, "Este convite foi revogado.");
        if (!long.TryParse(value, CultureInfo.InvariantCulture, out var epoch))
            return new InviteCheck(false, "Este convite não é válido.");
        if (DateTimeOffset.FromUnixTimeSeconds(epoch) <= _clock.GetUtcNow())
            return new InviteCheck(false, "Este convite expirou.");
        return new InviteCheck(true, "");
    }

    public async Task<bool> ConsumeAsync(string code, string newUserId, CancellationToken cancellationToken = default)
    {
        if (!TryKey(code, out var adminId, out var hash)) return false;
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider && token.Name == hash)
            .Select(token => token.Value).SingleOrDefaultAsync(cancellationToken);
        if (!long.TryParse(value, CultureInfo.InvariantCulture, out var epoch)
            || DateTimeOffset.FromUnixTimeSeconds(epoch) <= _clock.GetUtcNow()) return false;
        var used = $"U:{epoch}:{_clock.GetUtcNow().ToUnixTimeSeconds()}:{newUserId}";
        return await _db.UserTokens
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider
                && token.Name == hash && token.Value == value)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.Value, used), cancellationToken) == 1;
    }

    public async Task ReleaseAsync(string code, string userId, CancellationToken cancellationToken = default)
    {
        if (!TryKey(code, out var adminId, out var hash)) return;
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider && token.Name == hash)
            .Select(token => token.Value).SingleOrDefaultAsync(cancellationToken);
        var parts = value?.Split(':');
        if (parts is not { Length: 4 } || parts[0] != "U" || parts[3] != userId) return;
        await _db.UserTokens
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider
                && token.Name == hash && token.Value == value)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.Value, parts[1]), cancellationToken);
    }

    public async Task<IReadOnlyList<InviteSummary>> ListAsync(string adminId, CancellationToken cancellationToken = default)
    {
        var tokens = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider)
            .OrderByDescending(token => token.Name)
            .ToListAsync(cancellationToken);
        var usedIds = tokens.Select(token => token.Value?.Split(':'))
            .Where(parts => parts is { Length: 4 } && parts[0] == "U")
            .Select(parts => parts[3]).Distinct().ToList();
        var users = await _db.Users.AsNoTracking().Where(user => usedIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.Email, cancellationToken);
        return tokens.Select(token =>
        {
            var parts = token.Value?.Split(':') ?? [];
            var used = parts is { Length: 4 } && parts[0] == "U";
            var revoked = parts.Length > 0 && parts[0] == "R";
            var expiryText = used || revoked ? parts.ElementAtOrDefault(1) : token.Value;
            _ = long.TryParse(expiryText, CultureInfo.InvariantCulture, out var expiry);
            long usedAt = 0;
            if (used) long.TryParse(parts[2], CultureInfo.InvariantCulture, out usedAt);
            return new InviteSummary(token.Name, DateTimeOffset.FromUnixTimeSeconds(expiry),
                used ? DateTimeOffset.FromUnixTimeSeconds(usedAt) : null,
                used && users.TryGetValue(parts[3], out var email) ? email : null, revoked);
        }).ToList();
    }

    public async Task<bool> RevokeAsync(string adminId, string hash, CancellationToken cancellationToken = default)
    {
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider && token.Name == hash)
            .Select(token => token.Value).SingleOrDefaultAsync(cancellationToken);
        if (!long.TryParse(value, CultureInfo.InvariantCulture, out _)) return false;
        return await _db.UserTokens
            .Where(token => token.UserId == adminId && token.LoginProvider == Provider && token.Name == hash && token.Value == value)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.Value, "R:" + value), cancellationToken) == 1;
    }

    public static bool TryKey(string code, out string adminId, out string hash)
    {
        adminId = "";
        hash = "";
        if (string.IsNullOrWhiteSpace(code) || code.Length > 160) return false;
        var separator = code.IndexOf('.');
        if (separator is < 1 or > 64 || separator == code.Length - 1) return false;
        adminId = code[..separator];
        var secret = code[(separator + 1)..];
        if (secret.Length < 40 || secret.Any(ch => !char.IsAsciiLetterOrDigit(ch) && ch is not '-' and not '_')) return false;
        hash = Hash(code);
        return true;
    }

    private static string Hash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
}
