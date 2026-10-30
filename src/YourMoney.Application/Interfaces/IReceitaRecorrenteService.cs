using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces
{
    public interface IReceitaRecorrenteService
    {
        Task<ReceitaRecorrenteResponse> CriarAsync(ReceitaRecorrenteRequest request);
        Task<ListarReceitasRecorrentesResponse> ListarAsync(bool? ativas = null);
        Task<ReceitaRecorrenteResponse> ObterPorIdAsync(Guid id);
        Task<ReceitaRecorrenteResponse> AtualizarAsync(Guid id, ReceitaRecorrenteRequest request);
        Task DesativarAsync(Guid id);
        Task<ReceitaRecorrenteResponse> EncerrarAsync(Guid id, EncerrarReceitaRecorrenteRequest request);
        Task<ListarSugestoesReceitasRecorrentesResponse> ListarSugestoesAsync(int mes, int ano);
        Task<CriarReceitaResponse> ConfirmarSugestaoAsync(Guid ocorrenciaId, ConfirmarSugestaoReceitaRecorrenteRequest request);
        Task IgnorarSugestaoAsync(Guid ocorrenciaId);
        Task<ProjecaoReservaEmergenciaResponse> ObterProjecaoReservaAsync();
        Task<ListarSalariosElegiveisInvestimentoResponse> ListarElegiveisParaInvestimentoAsync();
    }
}
