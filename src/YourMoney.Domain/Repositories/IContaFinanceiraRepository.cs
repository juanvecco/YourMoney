using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IContaFinanceiraRepository
    {
        Task<ContaFinanceira> GetByIdAsync(Guid id);
        Task<ContaFinanceira> GetByIdAsync(Guid id, string usuarioId);
        Task AdicionarAsync(ContaFinanceira contaFinanceira);
        Task AtualizarAsync(ContaFinanceira contaFinanceira); 
        Task RemoverAsync(Guid id);
        Task RemoverAsync(Guid id, string usuarioId);
        Task<List<ContaFinanceira>> ListarAsync();
        Task<List<ContaFinanceira>> ListarAsync(string usuarioId);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteAsync(Guid id, string usuarioId);
    }
}
