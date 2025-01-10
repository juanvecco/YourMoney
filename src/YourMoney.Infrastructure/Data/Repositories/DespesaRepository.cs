using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;

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
    }
}
