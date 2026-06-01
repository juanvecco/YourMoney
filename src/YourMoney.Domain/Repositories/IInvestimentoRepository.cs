using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IInvestimentoRepository
    {
        Task<Investimento> GetByIdAsync(Guid id);
        Task<Investimento> GetByIdAsync(Guid id, string usuarioId);
        Task<List<Investimento>> GetAllAsync();
        Task<List<Investimento>> GetAllAsync(string usuarioId);
        Task<List<Investimento>> GetAtivosAsync();
        Task<List<Investimento>> GetAtivosAsync(string usuarioId);
        Task<List<Investimento>> GetByTipoAsync(string tipo);
        Task<List<Investimento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<List<Investimento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId);
        Task AdicionarAsync(Investimento investimento);
        Task AtualizarAsync(Investimento investimento);
        Task RemoverAsync(Guid id);
        Task RemoverAsync(Guid id, string usuarioId);
        Task<decimal> GetTotalInvestidoAsync();
        Task<decimal> GetTotalInvestidoAsync(string usuarioId);
        Task<decimal> GetTotalAtualAsync();
        Task<decimal> GetTotalAtualAsync(string usuarioId);
        Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano);
        Task<List<Investimento>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId);
        Task<List<Investimento>> ListarAsync();
        Task<List<Investimento>> ListarAsync(string usuarioId);
    }
}
