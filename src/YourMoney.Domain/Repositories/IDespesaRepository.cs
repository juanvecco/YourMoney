using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories
{
    public interface IDespesaRepository
    {
        Task<Despesa> GetByIdAsync(Guid id);
        Task<List<Despesa>> GetAllAsync();
        Task<List<Despesa>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<List<Despesa>> GetByMesAnoAsync(int mes, int ano);
        Task<List<Despesa>> ObterPorMesAnoAsync(int mes, int ano, Guid? idContaFinanceira);
        Task<List<Despesa>> GetByCategoriaAsync(Guid categoriaId);
        Task AdicionarAsync(Despesa despesa);
        Task AtualizarAsync(Despesa despesa); // Descomentado para permitir atualizações
        Task RemoverAsync(Guid id); 
        Task<List<Despesa>> ListarAsync();
    }
}