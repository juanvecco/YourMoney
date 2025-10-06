using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;

namespace YourMoney.Application.Interfaces
{ 
    public interface IInvestimentoService
    {
    Task AdicionarInvestimentoAsync(Investimento investimento);
    Task<Investimento> GetInvestimentoByIdAsync(Guid id);
    Task RemoverInvestimentoAsync(Guid id);
    Task AtualizarAsync(Investimento investimento);
    Task<List<Investimento>> ListarAsync();
    Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano);
    //Task<IEnumerable<InvestimentoDto>> ObterTodosAsync(int? usuarioId = null);
    //    Task<InvestimentoDto?> ObterPorIdAsync(int id, int? usuarioId = null);
    //    Task<InvestimentoDto> CriarAsync(CriarInvestimentoDto criarInvestimentoDto);
    //    Task<InvestimentoDto?> AtualizarAsync(int id, AtualizarInvestimentoDto atualizarInvestimentoDto, int? usuarioId = null);

    //    Task<ResumoInvestimentoDto> ObterResumoAsync(int? usuarioId = null);
    //    Task<IEnumerable<DistribuicaoCategoriaDto>> ObterDistribuicaoAsync(int? usuarioId = null);

    //    Task<IEnumerable<EvolucaoPatrimonioDto>> ObterEvolucaoPatrimonioAsync(
    //        DateTime dataInicio,
    //        DateTime dataFim,
    //        int? usuarioId = null);
    //    Task<IEnumerable<PerformanceInvestimentoDto>> ObterPerformanceAsync(int? usuarioId = null);

    //    Task AtualizarCotacoesAsync();

    //    Task<bool> AtualizarCotacaoAsync(int investimentoId);

    //    Task<decimal> CalcularRentabilidadeAsync(int investimentoId);

    //    Task<IEnumerable<InvestimentoDto>> ObterPorTipoAsync(string tipo, int? usuarioId = null);

    //    Task<bool> ValidarAtivoAsync(string ativo, string tipo);
    }
}

