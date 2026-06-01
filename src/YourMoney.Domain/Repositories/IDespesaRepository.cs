using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IDespesaRepository
    {
        Task<Despesa> GetByIdAsync(Guid id);
        Task<Despesa> GetByIdAsync(Guid id, string usuarioId);
        Task<List<Despesa>> GetAllAsync();
        Task<List<Despesa>> GetAllAsync(string usuarioId);
        Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId);
        Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano);
        Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano, string usuarioId);
        Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira);
        Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId, Guid? idContaFinanceira);
        Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId);
        Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId, string usuarioId);
        Task AdicionarAsync(Despesa despesa);
        Task AdicionarEmLoteAsync(IReadOnlyCollection<Despesa> despesas);
        Task AtualizarAsync(Despesa despesa); // Descomentado para permitir atualizações
        Task RemoverAsync(Guid id);
        Task RemoverAsync(Guid id, string usuarioId);
        Task<List<Despesa>> ListarAsync();
        Task<List<Despesa>> ListarAsync(string usuarioId);
    }
}
