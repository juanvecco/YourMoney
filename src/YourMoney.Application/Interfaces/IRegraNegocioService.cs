using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IRegraNegocioService
{
    Task<IReadOnlyList<RegraNegocioDto>> ListarAsync();
    Task<RegraNegocioDto> AtualizarAsync(string codigo, bool ativa);
    Task<bool> EstaAtivaAsync(string codigo);
}
