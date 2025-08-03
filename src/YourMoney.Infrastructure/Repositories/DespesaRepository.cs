using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class DespesaRepository : IDespesaRepository
    {
        private readonly AppDbContext _context;

        public DespesaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Despesa> GetByIdAsync(Guid id)
        {
            var despesa = await _context.Despesas
                //.Include(r => r.Categoria)
                .FirstOrDefaultAsync(r => r.Id == id);
            return despesa == null ? throw new InvalidOperationException("Despesa não encontrada.") : despesa;
        }

        public async Task<List<Despesa>> GetAllAsync()
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                .Where(r => r.Data >= dataInicio && r.Data <= dataFim)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano)
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                .Where(r => r.Data.Month == mes && r.Data.Year == ano)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira = null)
        {
            return await _context.Despesas
                .Where(d => d.Data.Month == mes && d.Data.Year == ano
                            && (idContaFinanceira == null || d.IdContaFinanceira == idContaFinanceira))
                .ToListAsync();
        }

        public async Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId)
        {
            return await _context.Despesas
                //.Include(r => r.Categoria)
                //.Where(r => r.CategoriaId == categoriaId)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Despesa despesa)
        {
            await _context.Despesas.AddAsync(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Despesa despesa)
        {
            _context.Despesas.Update(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Guid id)
        {
            var despesa = await GetByIdAsync(id);
            if (despesa != null)
            {
                _context.Despesas.Remove(despesa);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Despesa>> ListarAsync()
        {
            return await _context.Despesas
                //.Include(d => d.Categoria)
                .ToListAsync();
        }
    }
}