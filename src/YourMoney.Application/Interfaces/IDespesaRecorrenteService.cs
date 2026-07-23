using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces
{
    public interface IDespesaRecorrenteService
    {
        Task<DespesaRecorrenteResponse> CriarAsync(DespesaRecorrenteRequest request);
        Task<ListarDespesasRecorrentesResponse> ListarAsync(bool? ativas = null);
        Task<DespesaRecorrenteResponse> ObterPorIdAsync(Guid id);
        Task<DespesaRecorrenteResponse> AtualizarAsync(Guid id, DespesaRecorrenteRequest request);
        Task DesativarAsync(Guid id);
        Task<DespesaRecorrenteResponse> EncerrarAsync(Guid id, EncerrarDespesaRecorrenteRequest request);
        Task<ListarSugestoesDespesasRecorrentesResponse> ListarSugestoesAsync(int mes, int ano);
        Task<DespesaDTO> ConfirmarSugestaoAsync(Guid ocorrenciaId, ConfirmarSugestaoDespesaRecorrenteRequest request);
        Task IgnorarSugestaoAsync(Guid ocorrenciaId);
    }
}
