#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourMoney.Infrastructure.Data;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/usuarios")]
public sealed class AdminUsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _identity;
    private readonly IConfiguration _configuration;

    public AdminUsuariosController(ApplicationDbContext identity, IConfiguration configuration)
    {
        _identity = identity;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<PaginaUsuariosAdmin>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 25,
        [FromQuery] string? busca = null,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1 || tamanho is < 1 or > 100 || (long)(pagina - 1) * tamanho > int.MaxValue)
            return BadRequest(new { mensagem = "Informe uma página positiva e um tamanho entre 1 e 100." });

        Response.Headers.CacheControl = "no-store";

        var query = _identity.Users.AsNoTracking();
        var termo = busca?.Trim();
        if (!string.IsNullOrWhiteSpace(termo))
        {
            if (termo.Length > 120)
                return BadRequest(new { mensagem = "A busca deve ter até 120 caracteres." });
            var normalizedTerm = termo.ToUpperInvariant();
            query = query.Where(user => user.NormalizedEmail != null && user.NormalizedEmail.Contains(normalizedTerm));
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(user => user.NormalizedEmail)
            .ThenBy(user => user.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(user => new { user.Id, user.Email })
            .ToListAsync(cancellationToken);

        var ids = users.Select(user => user.Id).ToArray();
        var names = await _identity.UserClaims.AsNoTracking()
            .Where(claim => ids.Contains(claim.UserId) && claim.ClaimType == "name")
            .Select(claim => new { claim.UserId, claim.ClaimValue })
            .ToListAsync(cancellationToken);
        var nameById = names.GroupBy(claim => claim.UserId)
            .ToDictionary(group => group.Key, group => group.First().ClaimValue);

        var items = users.Select(user => new UsuarioAdmin(
            user.Id,
            nameById.GetValueOrDefault(user.Id) ?? string.Empty,
            user.Email ?? string.Empty)).ToArray();
        var resetAvailable = !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("SMTP_HOST") ?? _configuration["PasswordReset:Smtp:Host"])
            && !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("SMTP_FROM") ?? _configuration["PasswordReset:Smtp:From"]);
        return Ok(new PaginaUsuariosAdmin(items, pagina, tamanho, total, resetAvailable));
    }
}

public sealed record UsuarioAdmin(string Id, string Nome, string Email);
public sealed record PaginaUsuariosAdmin(
    IReadOnlyList<UsuarioAdmin> Itens,
    int Pagina,
    int Tamanho,
    int Total,
    bool RedefinicaoDisponivel);
