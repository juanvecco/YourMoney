using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
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
                //.Include(r => r.Categoria)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Receita>> GetAllAsync()
        {
            return await _context.Receitas
                //.Include(r => r.Categoria)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Receitas
                //.Include(r => r.Categoria)
                .Where(r => r.Data >= dataInicio && r.Data <= dataFim)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByMesAnoAsync(int mes, int ano)
        {
            return await _context.Receitas
                //.Include(r => r.Categoria)
                .Where(r => r.Data.Month == mes && r.Data.Year == ano)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano)
        {
            return await _context.Receitas
                .Where(r => r.Data.Month == mes && r.Data.Year == ano)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetByCategoriaAsync(Guid categoriaId)
        {
            return await _context.Receitas
                //.Include(r => r.Categoria)
                //.Where(r => r.CategoriaId == categoriaId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Receita>> GetPendentesAsync()
        {
            return await _context.Receitas
                //.Include(r => r.Categoria)
                //.Where(r => !r.Recebida)
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

        public async Task<decimal> GetTotalByMesAnoAsync(int mes, int ano)
        {
            return await _context.Receitas
                .Where(r => r.Data.Month == mes && r.Data.Year == ano)
                .SumAsync(r => r.Valor);
        }
    }
}