using YourMoney.Domain.Entities;

#nullable enable

namespace YourMoney.Domain.Repositories
{
    public interface IMetaMensalRepository
    {
        Task<MetaMensal?> GetByIdAsync(Guid id, string usuarioId);
        Task<List<MetaMensal>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId);
        Task AdicionarAsync(MetaMensal meta);
        Task AtualizarAsync(MetaMensal meta);
        Task RemoverAsync(Guid id, string usuarioId);
    }
}
