using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories;

public interface IRegraNegocioUsuarioRepository
{
    Task<IReadOnlyList<RegraNegocioUsuario>> ListarAsync(string usuarioId);
    Task SalvarAsync(string usuarioId, string codigo, bool ativa);
}
