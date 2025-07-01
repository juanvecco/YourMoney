using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("resumo")]
        public async Task<IActionResult> GetResumo([FromQuery] int mes = 0, [FromQuery] int ano = 0)
        {
            if (mes == 0) mes = DateTime.Now.Month;
            if (ano == 0) ano = DateTime.Now.Year;

            try
            {
                var dashboard = await _dashboardService.GetDashboardDataAsync(mes, ano);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("balanco-mensal")]
        public async Task<IActionResult> GetBalancoMensal([FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                var balanco = await _dashboardService.GetBalancoMensalAsync(mes, ano);
                return Ok(balanco);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("graficos/despesas")]
        public async Task<IActionResult> GetGraficoDespesas([FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                var grafico = await _dashboardService.GetGraficoDespesasPorCategoriaAsync(mes, ano);
                return Ok(grafico);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}