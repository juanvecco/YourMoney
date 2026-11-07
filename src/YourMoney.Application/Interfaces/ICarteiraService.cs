using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface ICarteiraService
{
    Task<IReadOnlyList<CarteiraResponseDTO>> ListarAsync();
    Task<CarteiraResponseDTO> CriarAsync(CarteiraRequestDTO request);
    Task<CarteiraResponseDTO> RenomearAsync(Guid id, CarteiraRequestDTO request);
    Task RemoverAsync(Guid id);
    Task<Guid> ObterIdPrincipalAsync();
}
