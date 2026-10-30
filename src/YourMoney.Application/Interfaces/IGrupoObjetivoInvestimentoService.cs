using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IGrupoObjetivoInvestimentoService
{
    Task<List<GrupoObjetivoInvestimentoResponse>> ListarAsync();
    Task<List<GrupoObjetivoInvestimentoResponse>> ListarAsync(bool incluirArquivados) => ListarAsync();
    Task<GrupoObjetivoInvestimentoResponse> ObterAsync(Guid id);
    Task<GrupoObjetivoInvestimentoResponse> CriarAsync(CriarGrupoObjetivoRequest request);
    Task<GrupoObjetivoInvestimentoResponse> RenomearAsync(Guid id, RenomearGrupoObjetivoRequest request);
    Task ExcluirAsync(Guid id, ExcluirGrupoObjetivoRequest request);
    Task<GrupoObjetivoInvestimentoResponse> ArquivarAsync(Guid id, VersaoGrupoObjetivoRequest request) => throw new NotSupportedException();
    Task<GrupoObjetivoInvestimentoResponse> ReativarAsync(Guid id, VersaoGrupoObjetivoRequest request) => throw new NotSupportedException();
    Task<List<GrupoObjetivoInvestimentoResumoResponse>> ObterResumoAsync(int mes, int ano, bool incluirArquivados = false) => throw new NotSupportedException();
}
