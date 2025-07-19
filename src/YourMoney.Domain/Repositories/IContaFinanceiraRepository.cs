using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IContaFinanceiraRepository
    {
        Task<ContaFinanceira> GetByIdAsync(Guid id);
        Task AdicionarAsync(ContaFinanceira contaFinanceira);
        Task AtualizarAsync(ContaFinanceira contaFinanceira); 
        Task RemoverAsync(Guid id);
        Task<List<ContaFinanceira>> ListarAsync();
    }
}
