using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IReceitaRepository
    {
        Task<Receita> GetByIdAsync(Guid id);
        Task<List<Receita>> GetAllAsync();
        Task<List<Receita>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<List<Receita>> GetByMesAnoAsync(int mes, int ano);
        Task<List<Receita>> ObterPorMesAnoAsync(int mes, int ano);
        Task<List<Receita>> GetByCategoriaAsync(Guid categoriaId);
        Task<List<Receita>> GetPendentesAsync();
        Task AdicionarAsync(Receita receita);
        Task AtualizarAsync(Receita receita);
        Task RemoverAsync(Guid id);
        Task<decimal> GetTotalByMesAnoAsync(int mes, int ano);
    }
}