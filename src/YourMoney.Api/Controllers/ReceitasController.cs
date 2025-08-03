using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceitasController : ControllerBase
    {
        private readonly IReceitaService _receitaService;

        public ReceitasController(IReceitaService receitaService)
        {
            _receitaService = receitaService;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarReceita([FromBody] Receita receita)
        {
            try
            {
                await _receitaService.AdicionarReceitaAsync(receita);
                return CreatedAtAction(nameof(AdicionarReceita), new { id = receita.Id }, receita);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("por-referencia")]
        public async Task<IActionResult> ObterPorReferencia([FromQuery] int mes, [FromQuery] int ano)
        {
            var receitas = await _receitaService.ObterPorMesAnoAsync(mes, ano);
            return Ok(receitas);
        }
    }
}
