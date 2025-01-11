using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Infrastructure.Data.Repositories
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
