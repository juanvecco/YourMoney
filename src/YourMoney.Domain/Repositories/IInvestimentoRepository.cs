using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IInvestimentoRepository
    {
        Task<Investimento> GetByIdAsync(Guid id);
        Task<List<Investimento>> GetAllAsync();
        Task<List<Investimento>> GetAtivosAsync();
        Task<List<Investimento>> GetByTipoAsync(string tipo);
        Task<List<Investimento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task AdicionarAsync(Investimento investimento);
        Task AtualizarAsync(Investimento investimento);
        Task RemoverAsync(Guid id);
        Task<decimal> GetTotalInvestidoAsync();
        Task<decimal> GetTotalAtualAsync();
        Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano);
        Task<List<Investimento>> ListarAsync();
    }
}