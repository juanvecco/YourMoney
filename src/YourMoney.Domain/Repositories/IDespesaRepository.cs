using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IDespesaRepository
    {
        //Task<Despesa> GetByIdAsync(Guid id);
        Task AdicionarAsync(Despesa despesa);
        //Task UpdateAsync(Despesa despesa);
        //Task DeleteAsync(Guid id);
        Task<List<Despesa>> GetAllAsync();
    }
}
