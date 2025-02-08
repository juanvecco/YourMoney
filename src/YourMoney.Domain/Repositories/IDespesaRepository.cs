using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IDespesaRepository
    {
        Task<Despesa> GetByIdAsync(Guid id); // Descomentado e ajustado o tipo de retorno
        Task AdicionarAsync(Despesa despesa);
        Task AtualizarAsync(Despesa despesa); // Descomentado para permitir atualizações
        Task RemoverAsync(Guid id);
        Task<List<Despesa>> ListarAsync();
    }
}