using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDTO> GetDashboardDataAsync(int mes, int ano);
        Task<List<GraficoDTO>> GetGraficoDespesasPorCategoriaAsync(int mes, int ano);
        Task<List<GraficoDTO>> GetGraficoReceitasPorCategoriaAsync(int mes, int ano);
        Task<BalancoMensalDTO> GetBalancoMensalAsync(int mes, int ano);
    }
}