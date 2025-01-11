using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<Despesa>> GetAllAsync()
        {
            // Exemplo usando Entity Framework
            return await _context.Despesas.ToListAsync();
        }
    }
}
