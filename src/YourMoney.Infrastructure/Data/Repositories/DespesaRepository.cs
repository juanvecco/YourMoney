using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace YourMoney.Infrastructure.Data.Repositories
{
    public class DespesaRepository : IDespesaRepository
    {
        private readonly YourMoneyDbContext _context;

        public DespesaRepository(YourMoneyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Despesa despesa)
        {
            _context.Despesas.Add(despesa);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Despesa>> GetAllAsync()
        {
            // Exemplo usando Entity Framework
            return await _context.Despesas.ToListAsync();
        }
    }
}
