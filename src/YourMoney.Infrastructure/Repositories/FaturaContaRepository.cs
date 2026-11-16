using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories;

public sealed class FaturaContaRepository : IFaturaContaRepository
{
    private readonly AppDbContext _context;
    public FaturaContaRepository(AppDbContext context) => _context = context;

    public Task<FaturaConta?> ObterAsync(Guid contaId, DateTime mesReferencia, string usuarioId) =>
        _context.FaturasConta.FirstOrDefaultAsync(f => f.IdContaFinanceira == contaId
            && f.MesReferencia == mesReferencia && f.UsuarioId == usuarioId);

    public async Task SalvarAsync(FaturaConta fatura)
    {
        if (_context.Entry(fatura).State == EntityState.Detached)
            await _context.FaturasConta.AddAsync(fatura);
        await _context.SaveChangesAsync();
    }
}
