using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Repositories
{
    public interface ICategoriaRepository
    {
        Task<Categoria> GetByIdAsync(Guid id);
        Task<Categoria> GetByIdAsync(Guid id, string usuarioId);
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
        Task RemoverAsync(Guid id);
        Task RemoverAsync(Guid id, string usuarioId);
        Task<List<Categoria>> GetAllAsync();
        Task<List<Categoria>> GetAllAsync(string usuarioId);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteAsync(Guid id, string usuarioId);
    }
}
