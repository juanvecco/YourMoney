using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<Categoria> GetByIdAsync(Guid id);
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
        Task RemoverAsync(Guid id);
        Task<List<Categoria>> GetAllAsync();
    }
}