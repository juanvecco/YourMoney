using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IDespesaService
    {
        Task AdicionarDespesaAsync(Despesa despesa);
        Task RemoverDespesaAsync(Guid id);
        Task<List<Despesa>> GetAllDespesasAsync(); // Renomeado para maior clareza
        Task<Despesa> GetDespesaByIdAsync(Guid id); // Método adicional para buscar por ID
    }
}