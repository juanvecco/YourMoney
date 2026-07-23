using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class InvestimentoController : ControllerBase
    {
        private readonly IInvestimentoService _investimentoService;

        public InvestimentoController(IInvestimentoService investimentoService)
        {
            _investimentoService = investimentoService;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarInvestimento([FromBody] CriarInvestimentoRequest request)
        {
            try
            {
                var response = await _investimentoService.CriarInvestimentoAsync(request);
                if (!response.CriadoAgora)
                    return Ok(response);
                return CreatedAtAction(
                    nameof(ObterPorId),
                    new { id = response.Id },
                    response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ConflitoOperacaoInvestimentoException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return FalhaInesperada();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverInvestimento(Guid id)
        {
            try
            {
                await _investimentoService.RemoverInvestimentoAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarInvestimentos()
        {
            return Ok(await _investimentoService.ListarAsync());
        }

        [HttpGet("consolidado")]
        public async Task<IActionResult> ObterConsolidado()
        {
            try { return Ok(await _investimentoService.ObterConsolidadoAsync()); }
            catch (Exception) { return FalhaInesperada(); }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            try { return Ok(await _investimentoService.ObterPorIdAsync(id)); }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return FalhaInesperada(); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarInvestimento(
            Guid id,
            [FromBody] AtualizarInvestimentoRequest request)
        {
            try
            {
                return Ok(await _investimentoService.AtualizarInvestimentoAsync(id, request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return FalhaInesperada();
            }
        }

        [HttpGet("por-referencia")]
        public async Task<IActionResult> ObterPorReferencia([FromQuery] int mes, [FromQuery] int ano)
        {
            return Ok(await _investimentoService.ObterPorMesAnoAsync(mes, ano));
        }

        private ObjectResult FalhaInesperada()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                title = "Não foi possível salvar o investimento.",
                status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
