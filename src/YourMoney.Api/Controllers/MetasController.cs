using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MetasController : ControllerBase
    {
        private readonly IMetaMensalService _metaMensalService;

        public MetasController(IMetaMensalService metaMensalService)
        {
            _metaMensalService = metaMensalService;
        }

        [HttpGet("resumo")]
        public async Task<IActionResult> ObterResumo([FromQuery] int? mes, [FromQuery] int? ano)
        {
            try
            {
                var resumo = await _metaMensalService.ObterResumoAsync(mes, ano);
                return Ok(resumo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarMetaMensalDTO request)
        {
            try
            {
                var meta = await _metaMensalService.CriarAsync(request);
                return CreatedAtAction(nameof(ObterResumo), new { mes = meta.MesReferencia.Month, ano = meta.MesReferencia.Year }, meta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível salvar a meta." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarMetaMensalDTO request)
        {
            try
            {
                var meta = await _metaMensalService.AtualizarAsync(id, request);
                return Ok(meta);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível atualizar a meta." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            try
            {
                await _metaMensalService.RemoverAsync(id);
                return NoContent();
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível excluir a meta." });
            }
        }
    }
}
