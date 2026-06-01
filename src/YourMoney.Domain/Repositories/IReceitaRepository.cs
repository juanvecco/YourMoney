using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IReceitaRepository
    {
        Task<Receita> GetByIdAsync(Guid id);
        Task<Receita> GetByIdAsync(Guid id, string usuarioId);
        Task<List<Receita>> GetAllAsync();
        Task<List<Receita>> GetAllAsync(string usuarioId);
        Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string usuarioId);
        Task<List<Receita>> GetByMesAnoAsync(int mes, int ano);
        Task<List<Receita>> GetByMesAnoAsync(int mes, int ano, string usuarioId);
        Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano);
        Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano, string usuarioId);
        Task<List<Receita>> GetByCategoriaAsync(Guid categoriaId);
        Task<List<Receita>> GetPendentesAsync();
        Task AdicionarAsync(Receita receita);
        Task AtualizarAsync(Receita receita);
        Task RemoverAsync(Guid id);
        Task RemoverAsync(Guid id, string usuarioId);
        Task<decimal> GetTotalByMesAnoAsync(int mes, int ano);
        Task<decimal> GetTotalByMesAnoAsync(int mes, int ano, string usuarioId);
        Task<List<Receita>> ListarAsync();
        Task<List<Receita>> ListarAsync(string usuarioId);
    }
}
