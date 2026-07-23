using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IInvestimentoService
    {
        Task AdicionarInvestimentoAsync(Investimento investimento);
        Task<CriarInvestimentoResponse> CriarInvestimentoAsync(CriarInvestimentoRequest request);
        Task<InvestimentoResponse> AtualizarInvestimentoAsync(Guid id, AtualizarInvestimentoRequest request);
        Task<Investimento> GetInvestimentoByIdAsync(Guid id);
        Task<InvestimentoResponse> ObterPorIdAsync(Guid id);
        Task<CarteiraInvestimentosConsolidadaResponse> ObterConsolidadoAsync();
        Task RemoverInvestimentoAsync(Guid id);
        Task AtualizarAsync(Investimento investimento);
        Task<List<InvestimentoResponse>> ListarAsync();
        Task<List<InvestimentoResponse>> ObterPorMesAnoAsync(int mes, int ano);
    }
}
