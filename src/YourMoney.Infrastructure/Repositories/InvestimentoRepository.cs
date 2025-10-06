using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class InvestimentoRepository : IInvestimentoRepository
    {
        private readonly AppDbContext _context;

        public InvestimentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Investimento> GetByIdAsync(Guid id)
        {
            return await _context.Investimentos.FindAsync(id);
        }

        public async Task<List<Investimento>> GetAllAsync()
        {
            return await _context.Investimentos
                .OrderByDescending(i => i.DataInvestimento)
                .ToListAsync();
        }

        public async Task<List<Investimento>> GetAtivosAsync()
        {
            return await _context.Investimentos
                .Where(i => i.Ativo)
                .OrderByDescending(i => i.DataInvestimento)
                .ToListAsync();
        }

        public async Task<List<Investimento>> GetByTipoAsync(string tipo)
        {
            return await _context.Investimentos
                .Where(i => i.Tipo == tipo)
                .OrderByDescending(i => i.DataInvestimento)
                .ToListAsync();
        }

        public async Task<List<Investimento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Investimentos
                .Where(i => i.DataInvestimento >= dataInicio && i.DataInvestimento <= dataFim)
                .OrderByDescending(i => i.DataInvestimento)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Investimento investimento)
        {
            await _context.Investimentos.AddAsync(investimento);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Investimento investimento)
        {
            _context.Investimentos.Update(investimento);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var investimento = await GetByIdAsync(id);
            if (investimento != null)
            {
                _context.Investimentos.Remove(investimento);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalInvestidoAsync()
        {
            return await _context.Investimentos
                .Where(i => i.Ativo)
                .SumAsync(i => i.ValorAtual);
        }

        public async Task<decimal> GetTotalAtualAsync()
        {
            return await _context.Investimentos
                .Where(i => i.Ativo)
                .SumAsync(i => i.ValorAtual);
        }

        public async Task<decimal> GetTotalByMesAnoAsync(int mes, int ano)
        {
            return await _context.Investimentos
                .Where(r => r.DataInvestimento.Month == mes && r.DataInvestimento.Year == ano)
                .SumAsync(r => r.ValorAtual);
        }

        public async Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano)
        {
            return await _context.Investimentos
                .Where(r => r.DataInvestimento.Month == mes && r.DataInvestimento.Year == ano)
                .ToListAsync();
        }

        public async Task<List<Investimento>> ListarAsync()
        {
            return await _context.Investimentos
                //.Include(d => d.Categoria)
                .ToListAsync();
        }
    }
}