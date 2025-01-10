using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DespesasController : ControllerBase
    {
        private readonly IDespesaService _despesaService;

        public DespesasController(IDespesaService despesaService)
        {
            _despesaService = despesaService;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarDespesa([FromBody] Despesa despesa)
        {
            try
            {
                await _despesaService.AdicionarDespesaAsync(despesa);
                return CreatedAtAction(nameof(AdicionarDespesa), new { id = despesa.Id }, despesa);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
