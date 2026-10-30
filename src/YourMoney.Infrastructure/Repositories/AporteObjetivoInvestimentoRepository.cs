using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories;

public sealed class AporteObjetivoInvestimentoRepository : IAporteObjetivoInvestimentoRepository
{
    private readonly AppDbContext _context;
    public AporteObjetivoInvestimentoRepository(AppDbContext context) => _context = context;

    public Task<AporteObjetivoInvestimento?> ObterPorOperacaoAsync(Guid operacaoId, string usuarioId) =>
        _context.AportesObjetivoInvestimento.AsNoTracking().FirstOrDefaultAsync(a => a.OperacaoId == operacaoId && a.UsuarioId == usuarioId);
    public Task<AporteObjetivoInvestimento?> ObterAsync(Guid id, string usuarioId) =>
        _context.AportesObjetivoInvestimento.FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId == usuarioId);
    public Task<List<AporteObjetivoInvestimento>> ListarPorMetaAsync(Guid metaId, string usuarioId) =>
        _context.AportesObjetivoInvestimento.AsNoTracking().Where(a => a.MetaMensalId == metaId && a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.DataConfirmacao).ThenByDescending(a => a.CriadoEm).ToListAsync();
    public Task<List<AporteObjetivoInvestimento>> ListarPorMesAsync(int mes, int ano, string usuarioId) =>
        _context.AportesObjetivoInvestimento.AsNoTracking().Where(a => a.UsuarioId == usuarioId && a.MesReferencia.Month == mes && a.MesReferencia.Year == ano).ToListAsync();
    public Task<List<AporteObjetivoInvestimento>> ListarPorObjetivoAsync(Guid objetivoId, string usuarioId) =>
        _context.AportesObjetivoInvestimento.AsNoTracking().Where(a => a.UsuarioId == usuarioId && a.GrupoObjetivoInvestimentoId == objetivoId)
            .OrderByDescending(a => a.DataConfirmacao).ToListAsync();
    public Task<bool> ExistePorMetaAsync(Guid metaId, string usuarioId) =>
        _context.AportesObjetivoInvestimento.AnyAsync(a => a.MetaMensalId == metaId && a.UsuarioId == usuarioId);

    public async Task AdicionarAsync(AporteObjetivoInvestimento aporte)
    {
        _context.AportesObjetivoInvestimento.Add(aporte);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException) { throw new InvalidOperationException("Já existe um aporte com este identificador de operação."); }
    }

    public async Task AtualizarAsync(AporteObjetivoInvestimento aporte)
    {
        _context.AportesObjetivoInvestimento.Update(aporte);
        await _context.SaveChangesAsync();
    }
}
