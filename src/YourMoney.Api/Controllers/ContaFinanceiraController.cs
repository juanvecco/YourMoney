using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContaFinanceiraController : ControllerBase
    {
        private readonly IContaFinanceiraService _contaFinanceiraService;
        public ContaFinanceiraController(IContaFinanceiraService contaFinanceiraService)
        {
            _contaFinanceiraService = contaFinanceiraService;
        }

        [HttpPost]

        public async Task<IActionResult> AdicionarContaFinanceira([FromBody] ContaFinanceira contaFinanceira)
        {
            try
            {
                await _contaFinanceiraService.AdicionarContaFinanceiraAsync(contaFinanceira);
                return CreatedAtAction(nameof(AdicionarContaFinanceira), new { id = contaFinanceira.Id }, contaFinanceira);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverContaFinanceira(Guid id)
        {
            try
            {
                await _contaFinanceiraService.RemoverContaFinanceiraAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarContaFinanceiras()
        {
            try
            {
                var contasFinanceiras = await _contaFinanceiraService.ListarAsync();
                return Ok(contasFinanceiras);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarContaFinanceira(Guid id, [FromBody] ContaFinanceira contaFinanceira)
        {
            if (id != contaFinanceira.Id)
            {
                return BadRequest("O ID da URL não corresponde ao ID da conta financeira.");
            }

            try
            {
                await _contaFinanceiraService.AtualizarAsync(contaFinanceira);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
