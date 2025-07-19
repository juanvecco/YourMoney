using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Repositories
{
    public class ContaFinanceiraRepository : IContaFinanceiraRepository
    {
        private readonly AppDbContext _context;

        public ContaFinanceiraRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContaFinanceira> GetByIdAsync(Guid id)
        {
            var contaFinanceira = await _context.ContaFinanceira
                .FirstOrDefaultAsync(r => r.Id == id);
            return contaFinanceira == null ? throw new InvalidOperationException("Conta Financeira não encontrada.") : contaFinanceira;
        }

        public async Task AdicionarAsync(ContaFinanceira contaFinanceira)
        {
            await _context.ContaFinanceira.AddAsync(contaFinanceira);
            await _context.SaveChangesAsync();
        }
        public async Task AtualizarAsync(ContaFinanceira contaFinanceira)
        {

            _context.ContaFinanceira.Update(contaFinanceira);
            await _context.SaveChangesAsync();
        }
        public async Task<List<ContaFinanceira>> ListarAsync()
        {
            return await _context.ContaFinanceira
                .ToListAsync();
        }
        public async Task RemoverAsync(Guid id)
        {

            var contaFinanceira = await GetByIdAsync(id);
            if (contaFinanceira != null)
            {
                _context.ContaFinanceira.Remove(contaFinanceira);
                await _context.SaveChangesAsync();
            }
        }
    }
}
