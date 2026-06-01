using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Repositories
{
    public interface IMetaRepository
    {
        Task<Meta> GetByIdAsync(Guid id);
        Task<Meta> GetByIdAsync(Guid id, string usuarioId);
        Task<List<Meta>> GetAllAsync();
        Task<List<Meta>> GetByStatusAsync(StatusMeta status);
        Task<List<Meta>> GetAtivasAsync();
        Task<List<Meta>> GetAtivasAsync(string usuarioId);
        Task<List<Meta>> GetVencendoAsync(int diasProximoVencimento = 30);
        Task AdicionarAsync(Meta meta);
        Task AtualizarAsync(Meta meta);
        Task RemoverAsync(Guid id);
    }
}
