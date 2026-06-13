using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{
    public interface IInvestimentoService
    {
        Task AdicionarInvestimentoAsync(Investimento investimento);
        Task<CriarInvestimentoResponse> CriarInvestimentoAsync(CriarInvestimentoRequest request);
        Task<Investimento> GetInvestimentoByIdAsync(Guid id);
        Task RemoverInvestimentoAsync(Guid id);
        Task AtualizarAsync(Investimento investimento);
        Task<List<Investimento>> ListarAsync();
        Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano);
    }
}
