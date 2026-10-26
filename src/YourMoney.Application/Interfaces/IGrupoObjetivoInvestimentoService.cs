using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IGrupoObjetivoInvestimentoService
{
    Task<List<GrupoObjetivoInvestimentoResponse>> ListarAsync();
    Task<GrupoObjetivoInvestimentoResponse> ObterAsync(Guid id);
    Task<GrupoObjetivoInvestimentoResponse> CriarAsync(CriarGrupoObjetivoRequest request);
    Task<GrupoObjetivoInvestimentoResponse> RenomearAsync(Guid id, RenomearGrupoObjetivoRequest request);
    Task ExcluirAsync(Guid id, ExcluirGrupoObjetivoRequest request);
}
