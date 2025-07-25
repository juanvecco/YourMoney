using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Repositories
{
    public interface ICategoriaRepository
    {
        Task<Categoria> GetByIdAsync(Guid id);
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
        Task RemoverAsync(Guid id);
        Task<List<Categoria>> GetAllAsync();
    }
}