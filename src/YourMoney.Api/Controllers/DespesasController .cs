using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Queries.Handlers;
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverDespesa(Guid id)
        {
            try
            {
                await _despesaService.RemoverDespesaAsync(id);
                return NoContent(); // Retorna 204 No Content
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message); // Retorna 404 Not Found
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var expenses = await _despesaService.Send(new GetExpensesQuery());
        //    return Ok(expenses);
        //}
    }
}
