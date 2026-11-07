using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories;

public interface ICarteiraRepository
{
    Task<List<Carteira>> ListarAsync(string usuarioId);
    Task<Carteira?> ObterAsync(Guid id, string usuarioId);
    Task<Carteira?> ObterPrincipalAsync(string usuarioId);
    Task<Carteira> ObterOuCriarPrincipalAsync(string usuarioId);
    Task<bool> NomeExisteAsync(string usuarioId, string nomeNormalizado, Guid? excetoId = null);
    Task<bool> PossuiDependenciasAsync(Guid carteiraId, string usuarioId);
    Task AdicionarAsync(Carteira carteira);
    Task AtualizarAsync(Carteira carteira);
    Task RemoverAsync(Carteira carteira);
}
