using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories;

public sealed class RegraNegocioUsuarioRepository : IRegraNegocioUsuarioRepository
{
    private readonly AppDbContext _db;

    public RegraNegocioUsuarioRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<RegraNegocioUsuario>> ListarAsync(string usuarioId) =>
        await _db.RegrasNegocioUsuario.AsNoTracking()
            .Where(rule => rule.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task SalvarAsync(string usuarioId, string codigo, bool ativa)
    {
        var existing = await _db.RegrasNegocioUsuario
            .SingleOrDefaultAsync(rule => rule.UsuarioId == usuarioId && rule.Codigo == codigo);
        if (existing is not null)
        {
            existing.Alterar(ativa);
            await _db.SaveChangesAsync();
            return;
        }

        var created = new RegraNegocioUsuario(usuarioId, codigo, ativa);
        _db.RegrasNegocioUsuario.Add(created);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _db.Entry(created).State = EntityState.Detached;
            var updated = await _db.RegrasNegocioUsuario
                .Where(rule => rule.UsuarioId == usuarioId && rule.Codigo == codigo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(rule => rule.Ativa, ativa)
                    .SetProperty(rule => rule.AtualizadaEm, DateTimeOffset.UtcNow));
            if (updated == 0) throw;
        }
    }
}
