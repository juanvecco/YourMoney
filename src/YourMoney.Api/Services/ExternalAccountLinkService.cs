using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using YourMoney.Api.Configuration;
using YourMoney.Api.Models;
using YourMoney.Infrastructure.Data;

namespace YourMoney.Api.Services;

public sealed class ExternalAccountLinkService
{
    private const string StartProvider = "YourMoneyGoogleLinkStart";
    private const string PendingProvider = "YourMoneyGoogleLinkPending";
    private static readonly TimeSpan TicketLifetime = TimeSpan.FromMinutes(5);
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;
    private readonly IConfiguration _configuration;

    public ExternalAccountLinkService(
        ApplicationDbContext db,
        UserManager<IdentityUser> users,
        IConfiguration configuration)
    {
        _db = db;
        _users = users;
        _configuration = configuration;
    }

    public async Task<ExternalAccountLinkStatus?> GetStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return null;
        var logins = await _users.GetLoginsAsync(user);
        return new ExternalAccountLinkStatus(
            ExternalProviderConfiguration.GoogleAvailable(_configuration),
            logins.Any(login => string.Equals(login.LoginProvider, "google", StringComparison.OrdinalIgnoreCase)));
    }

    public async Task<ExternalAccountLinkResult> PrepareAsync(
        string userId,
        string currentPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return new(false, ExternalAccountLinkFailure.AccountNotFound);
        if (await _users.IsLockedOutAsync(user)) return new(false, ExternalAccountLinkFailure.AccountLocked);

        if (!await _users.CheckPasswordAsync(user, currentPassword))
        {
            await _users.AccessFailedAsync(user);
            return new(false, ExternalAccountLinkFailure.InvalidPassword);
        }
        await _users.ResetAccessFailedCountAsync(user);

        var logins = await _users.GetLoginsAsync(user);
        if (logins.Any(login => string.Equals(login.LoginProvider, "google", StringComparison.OrdinalIgnoreCase)))
            return new(false, ExternalAccountLinkFailure.AlreadyLinked);

        var oldTickets = await _db.UserTokens
            .Where(token => token.UserId == userId && (token.LoginProvider == StartProvider || token.LoginProvider == PendingProvider))
            .ToListAsync(cancellationToken);
        if (oldTickets.Count > 0)
        {
            _db.UserTokens.RemoveRange(oldTickets);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var nonce = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTimeOffset.UtcNow.Add(TicketLifetime);
        await SaveTicketAsync(userId, StartProvider, nonce, expiresAt, cancellationToken);
        return new(true, Ticket: nonce, ExpiresAt: expiresAt);
    }

    public Task<bool> ConsumeStartTicketAsync(string userId, string nonce, CancellationToken cancellationToken = default) =>
        ConsumeTicketAsync(userId, StartProvider, nonce, cancellationToken);

    public async Task<ExternalAccountLinkResult> CreatePendingAsync(
        string userId,
        string providerKey,
        string verifiedEmail,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return new(false, ExternalAccountLinkFailure.AccountNotFound);
        if (await _users.IsLockedOutAsync(user)) return new(false, ExternalAccountLinkFailure.AccountLocked);
        if (string.IsNullOrWhiteSpace(providerKey)
            || !IsVerifiedEmailMatch(user.Email, verifiedEmail, "true", _users.KeyNormalizer!))
            return new(false, ExternalAccountLinkFailure.EmailMismatch);

        var owner = await _users.FindByLoginAsync("google", providerKey);
        if (owner != null)
            return new(false, owner.Id == userId
                ? ExternalAccountLinkFailure.AlreadyLinked
                : ExternalAccountLinkFailure.IdentityConflict);

        var nonce = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTimeOffset.UtcNow.Add(TicketLifetime);
        await SaveTicketAsync(userId, PendingProvider, nonce, expiresAt, cancellationToken);
        return new(true, Ticket: nonce, ExpiresAt: expiresAt);
    }

    public async Task<PendingGoogleLinkResponse?> GetPendingAsync(
        string userId,
        GoogleLinkPendingTicket pending,
        CancellationToken cancellationToken = default)
    {
        if (!IsPendingTicketFor(userId, pending)
            || !await HasLiveTicketAsync(userId, PendingProvider, pending.Nonce, cancellationToken))
            return null;

        return new PendingGoogleLinkResponse("google", MaskEmail(pending.Email), await GetExpiry(userId, PendingProvider, pending.Nonce));
    }

    public async Task<ExternalAccountLinkResult> ConfirmAsync(
        string userId,
        GoogleLinkPendingTicket pending,
        CancellationToken cancellationToken = default)
    {
        if (!IsPendingTicketFor(userId, pending))
            return new(false, ExternalAccountLinkFailure.InvalidTicket);

        var user = await _users.FindByIdAsync(userId);
        if (user == null) return new(false, ExternalAccountLinkFailure.AccountNotFound);
        if (await _users.IsLockedOutAsync(user)) return new(false, ExternalAccountLinkFailure.AccountLocked);
        if (!IsVerifiedEmailMatch(user.Email, pending.Email, "true", _users.KeyNormalizer!))
            return new(false, ExternalAccountLinkFailure.EmailMismatch);
        if (!await HasLiveTicketAsync(userId, PendingProvider, pending.Nonce, cancellationToken))
            return new(false, ExternalAccountLinkFailure.InvalidTicket);

        var owner = await _users.FindByLoginAsync("google", pending.ProviderKey);
        if (owner != null)
        {
            if (!await ConsumeTicketAsync(userId, PendingProvider, pending.Nonce, cancellationToken))
                return new(false, ExternalAccountLinkFailure.InvalidTicket);
            return owner.Id == userId
                ? new(true, Status: await GetStatusAsync(userId, cancellationToken))
                : new(false, ExternalAccountLinkFailure.IdentityConflict);
        }

        if (!await ConsumeTicketAsync(userId, PendingProvider, pending.Nonce, cancellationToken))
            return new(false, ExternalAccountLinkFailure.InvalidTicket);

        var result = await _users.AddLoginAsync(user, new UserLoginInfo("google", pending.ProviderKey, "Google"));
        if (!result.Succeeded)
        {
            // Identity's unique provider/subject key is the final guard against a
            // concurrent link of the same Google account to a different user.
            var currentOwner = await _users.FindByLoginAsync("google", pending.ProviderKey);
            if (currentOwner != null && currentOwner.Id == userId)
                return new(true, Status: await GetStatusAsync(userId, cancellationToken));
            return new(false, ExternalAccountLinkFailure.IdentityConflict);
        }

        return new(true, Status: await GetStatusAsync(userId, cancellationToken));
    }

    public Task<bool> CancelPendingAsync(string userId, GoogleLinkPendingTicket? pending, CancellationToken cancellationToken = default)
    {
        if (pending == null || !IsPendingTicketFor(userId, pending)) return Task.FromResult(false);
        return ConsumeTicketAsync(userId, PendingProvider, pending.Nonce, cancellationToken);
    }

    public static bool IsVerifiedEmailMatch(
        string? accountEmail,
        string? providerEmail,
        string? emailVerified,
        ILookupNormalizer normalizer)
    {
        if (!string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(accountEmail)
            || string.IsNullOrWhiteSpace(providerEmail))
            return false;

        return string.Equals(normalizer.NormalizeEmail(accountEmail), normalizer.NormalizeEmail(providerEmail), StringComparison.Ordinal);
    }

    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "";
        var at = email.LastIndexOf('@');
        if (at <= 0 || at == email.Length - 1) return "";
        return $"{email[0]}***{email[at..]}";
    }

    private static bool IsPendingTicketFor(string userId, GoogleLinkPendingTicket pending) =>
        pending != null
        && string.Equals(userId, pending.UserId, StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(pending.ProviderKey)
        && !string.IsNullOrWhiteSpace(pending.Nonce)
        && !string.IsNullOrWhiteSpace(pending.Email);

    private async Task SaveTicketAsync(
        string userId,
        string provider,
        string nonce,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        _db.UserTokens.Add(new IdentityUserToken<string>
        {
            UserId = userId,
            LoginProvider = provider,
            Name = HashNonce(nonce),
            Value = expiresAt.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> HasLiveTicketAsync(
        string userId,
        string provider,
        string nonce,
        CancellationToken cancellationToken)
    {
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.LoginProvider == provider && token.Name == HashNonce(nonce))
            .Select(token => token.Value)
            .SingleOrDefaultAsync(cancellationToken);
        return long.TryParse(value, out var expires)
            && expires > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private async Task<DateTimeOffset> GetExpiry(string userId, string provider, string nonce)
    {
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.LoginProvider == provider && token.Name == HashNonce(nonce))
            .Select(token => token.Value)
            .SingleOrDefaultAsync();
        return long.TryParse(value, out var expires)
            ? DateTimeOffset.FromUnixTimeSeconds(expires)
            : DateTimeOffset.MinValue;
    }

    private async Task<bool> ConsumeTicketAsync(
        string userId,
        string provider,
        string nonce,
        CancellationToken cancellationToken)
    {
        var hash = HashNonce(nonce);
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.LoginProvider == provider && token.Name == hash)
            .Select(token => token.Value)
            .SingleOrDefaultAsync(cancellationToken);
        if (!long.TryParse(value, out var expires)
            || expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return false;

        var consumed = await _db.UserTokens
            .Where(token => token.UserId == userId && token.LoginProvider == provider
                && token.Name == hash && token.Value == value)
            .ExecuteDeleteAsync(cancellationToken);
        return consumed == 1;
    }

    private static string HashNonce(string nonce) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(nonce)));
}
