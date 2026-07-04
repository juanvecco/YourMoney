using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class ReceitaRepository : IReceitaRepository
    {
        private readonly AppDbContext _context;

        public ReceitaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Receita> GetByIdAsync(Guid id)
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Receita> GetByIdAsync(Guid id, string usuarioId)
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);
        }

        public async Task<List<Receita>> GetAllAsync()
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetAllAsync(string usuarioId)
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .Where(r => r.Data >= dataInicio && r.Data <= dataFim)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId)
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .Where(r => r.UsuarioId == usuarioId && r.Data >= dataInicio && r.Data <= dataFim)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByMesAnoAsync(int mes, int ano)
        {
            return await ReceitaPorReferencia(mes, ano)
                .Include(r => r.DespesaVinculada)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return await ReceitaPorReferencia(mes, ano, usuarioId)
                .Include(r => r.DespesaVinculada)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano)
        {
            return await ReceitaPorReferencia(mes, ano)
                .Include(r => r.DespesaVinculada)
                .ToListAsync();
        }

        public async Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return await ReceitaPorReferencia(mes, ano, usuarioId)
                .Include(r => r.DespesaVinculada)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByCategoriaAsync(Guid categoriaId)
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetPendentesAsync()
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .OrderBy(r => r.Data)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Receita receita)
        {
            await _context.Receitas.AddAsync(receita);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Receita receita)
        {
            _context.Receitas.Update(receita);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var receita = await GetByIdAsync(id);
            if (receita != null)
            {
                _context.Receitas.Remove(receita);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoverAsync(Guid id, string usuarioId)
        {
            var receita = await GetByIdAsync(id, usuarioId);
            if (receita != null)
            {
                _context.Receitas.Remove(receita);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalByMesAnoAsync(int mes, int ano)
        {
            return await ReceitaPorReferencia(mes, ano).SumAsync(r => r.Valor);
        }

        public async Task<decimal> GetTotalByMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return await GetTotalBrutoByMesAnoAsync(mes, ano, usuarioId);
        }

        public async Task<decimal> GetTotalBrutoByMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return await ReceitaPorReferencia(mes, ano, usuarioId).SumAsync(r => r.Valor);
        }

        public async Task<decimal> GetTotalElegivelMetasByMesAnoAsync(int mes, int ano, string usuarioId)
        {
            return await ReceitaPorReferencia(mes, ano, usuarioId)
                .Where(r => r.Natureza == NaturezaReceita.RendaDisponivel)
                .SumAsync(r => r.Valor);
        }

        public async Task<decimal> GetTotalReembolsadoPorDespesaAsync(Guid despesaId, string usuarioId, Guid? receitaIgnoradaId = null)
        {
            return await _context.Receitas
                .Where(r => r.UsuarioId == usuarioId
                    && r.Natureza == NaturezaReceita.Reembolso
                    && r.DespesaVinculadaId == despesaId
                    && (!receitaIgnoradaId.HasValue || r.Id != receitaIgnoradaId.Value))
                .SumAsync(r => r.Valor);
        }

        public async Task<Dictionary<Guid, decimal>> GetTotaisReembolsadosPorDespesasAsync(IReadOnlyCollection<Guid> despesaIds, string usuarioId)
        {
            if (despesaIds.Count == 0)
                return new Dictionary<Guid, decimal>();

            return await _context.Receitas
                .Where(r => r.UsuarioId == usuarioId
                    && r.Natureza == NaturezaReceita.Reembolso
                    && r.DespesaVinculadaId.HasValue
                    && despesaIds.Contains(r.DespesaVinculadaId.Value))
                .GroupBy(r => r.DespesaVinculadaId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(r => r.Valor));
        }

        public async Task<List<Receita>> ListarAsync()
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .ToListAsync();
        }

        public async Task<List<Receita>> ListarAsync(string usuarioId)
        {
            return await _context.Receitas
                .Include(r => r.DespesaVinculada)
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();
        }

        private IQueryable<Receita> ReceitaPorReferencia(int mes, int ano)
        {
            return _context.Receitas.Where(r =>
                (r.MesReferencia.HasValue
                    && r.MesReferencia.Value.Month == mes
                    && r.MesReferencia.Value.Year == ano)
                || (!r.MesReferencia.HasValue && r.Data.Month == mes && r.Data.Year == ano));
        }

        private IQueryable<Receita> ReceitaPorReferencia(int mes, int ano, string usuarioId)
        {
            return ReceitaPorReferencia(mes, ano).Where(r => r.UsuarioId == usuarioId);
        }
    }
}
