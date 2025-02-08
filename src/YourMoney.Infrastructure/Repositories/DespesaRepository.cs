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

        public async Task AdicionarAsync(Despesa despesa)
        {
            await _context.TbDespesa.AddAsync(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Despesa>> ListarAsync()
        {
            return await _context.TbDespesa.ToListAsync();
        }

        public async Task<Despesa> GetByIdAsync(Guid id)
        {
            var despesa = await _context.TbDespesa.FindAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            return despesa;
        }

        public async Task RemoverAsync(Guid id)
        {
            var despesa = await _context.TbDespesa.FindAsync(id);
            if (despesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            _context.TbDespesa.Remove(despesa);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Despesa despesa)
        {
            var existingDespesa = await _context.TbDespesa.FindAsync(despesa.Id);
            if (existingDespesa == null)
            {
                throw new InvalidOperationException("Despesa não encontrada.");
            }
            _context.Entry(existingDespesa).CurrentValues.SetValues(despesa);
            await _context.SaveChangesAsync();
        }
    }
}