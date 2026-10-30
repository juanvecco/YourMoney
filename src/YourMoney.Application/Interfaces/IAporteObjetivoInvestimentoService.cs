using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IAporteObjetivoInvestimentoService
{
    Task<List<AporteObjetivoInvestimentoDTO>> ListarAsync(Guid metaId);
    Task<AporteObjetivoInvestimentoDTO> ConfirmarAsync(Guid metaId, ConfirmarAporteObjetivoRequest request);
    Task<AporteObjetivoInvestimentoDTO> AlocarAsync(Guid metaId, Guid aporteId, AlocarAporteObjetivoRequest request);
}
