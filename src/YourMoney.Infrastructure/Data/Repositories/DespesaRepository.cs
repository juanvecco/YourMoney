using YourMoney.Domain.Interfaces.Repositories;
using YourMoney.Domain.Models;

namespace YourMoney.Infrastructure.Data.Repositories
{
    public class DespesaRepository : IDespesaRepository
    {
        private readonly YourMoneyDbContext _context;

        public DespesaRepository(YourMoneyDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Despesa despesa)
        {
            _context.Despesas.Add(despesa);
            await _context.SaveChangesAsync();
        }
    }
}
