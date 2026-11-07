using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories;

public sealed class CarteiraRepository : ICarteiraRepository
{
    private readonly AppDbContext _context;

    public CarteiraRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Carteira>> ListarAsync(string usuarioId) => _context.Carteiras
        .Where(wallet => wallet.UsuarioId == usuarioId)
        .OrderByDescending(wallet => wallet.Principal)
        .ThenBy(wallet => wallet.NomeNormalizado)
        .ToListAsync();

    public Task<Carteira?> ObterAsync(Guid id, string usuarioId) => _context.Carteiras
        .SingleOrDefaultAsync(wallet => wallet.Id == id && wallet.UsuarioId == usuarioId);

    public Task<Carteira?> ObterPrincipalAsync(string usuarioId) => _context.Carteiras
        .SingleOrDefaultAsync(wallet => wallet.UsuarioId == usuarioId && wallet.Principal);

    public async Task<Carteira> ObterOuCriarPrincipalAsync(string usuarioId)
    {
        var principal = await ObterPrincipalAsync(usuarioId);
        if (principal is not null)
            return principal;

        var nova = new Carteira(usuarioId, "Minha carteira", principal: true);
        _context.Carteiras.Add(nova);
        try
        {
            await _context.SaveChangesAsync();
            return nova;
        }
        catch (DbUpdateException)
        {
            _context.Entry(nova).State = EntityState.Detached;
            principal = await ObterPrincipalAsync(usuarioId);
            if (principal is not null)
                return principal;
            throw;
        }
    }

    public Task<bool> NomeExisteAsync(string usuarioId, string nomeNormalizado, Guid? excetoId = null) =>
        _context.Carteiras.AnyAsync(wallet => wallet.UsuarioId == usuarioId
            && wallet.NomeNormalizado == nomeNormalizado
            && (!excetoId.HasValue || wallet.Id != excetoId.Value));

    public async Task<bool> PossuiDependenciasAsync(Guid carteiraId, string usuarioId) =>
        await _context.ContaFinanceira.AnyAsync(account => account.CarteiraId == carteiraId && account.UsuarioId == usuarioId)
        || await _context.PluggyConnections.AnyAsync(connection => connection.CarteiraId == carteiraId && connection.UsuarioId == usuarioId);

    public async Task AdicionarAsync(Carteira carteira)
    {
        _context.Carteiras.Add(carteira);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Carteira carteira)
    {
        _context.Carteiras.Update(carteira);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Carteira carteira)
    {
        _context.Carteiras.Remove(carteira);
        await _context.SaveChangesAsync();
    }
}
