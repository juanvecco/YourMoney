using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Repositories
{
    public interface ICategoriaRepository
    {
        Task<Categoria> GetByIdAsync(Guid id);
        Task<List<Categoria>> GetAllAsync();
        Task<List<Categoria>> GetByTipoAsync(TipoTransacao tipo);
        Task<List<Categoria>> GetAtivasAsync();
        Task AdicionarAsync(Categoria categoria);
        Task AtualizarAsync(Categoria categoria);
        Task RemoverAsync(Guid id);
        Task<bool> ExisteAsync(Guid id);
        Task<bool> ExisteNomeAsync(string nome, TipoTransacao tipo, Guid? ignorarId = null);
    }
}