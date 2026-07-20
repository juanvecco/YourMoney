using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReceitasRecorrentesController : ControllerBase
    {
        private readonly IReceitaRecorrenteService _service;

        public ReceitasRecorrentesController(IReceitaRecorrenteService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] ReceitaRecorrenteRequest request)
        {
            try
            {
                var response = await _service.CriarAsync(request);
                return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool? ativas = null)
        {
            try { return Ok(await _service.ListarAsync(ativas)); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            try { return Ok(await _service.ObterPorIdAsync(id)); }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] ReceitaRecorrenteRequest request)
        {
            try { return Ok(await _service.AtualizarAsync(id, request)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpPatch("{id:guid}/desativar")]
        public async Task<IActionResult> Desativar(Guid id)
        {
            try
            {
                await _service.DesativarAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpPatch("{id:guid}/encerrar")]
        public async Task<IActionResult> Encerrar(Guid id, [FromBody] EncerrarReceitaRecorrenteRequest request)
        {
            try { return Ok(await _service.EncerrarAsync(id, request)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpGet("sugestoes")]
        public async Task<IActionResult> ListarSugestoes([FromQuery] int mes, [FromQuery] int ano)
        {
            try { return Ok(await _service.ListarSugestoesAsync(mes, ano)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpPost("sugestoes/{ocorrenciaId:guid}/confirmar")]
        public async Task<IActionResult> Confirmar(
            Guid ocorrenciaId,
            [FromBody] ConfirmarSugestaoReceitaRecorrenteRequest request)
        {
            try
            {
                var response = await _service.ConfirmarSugestaoAsync(ocorrenciaId, request);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("finalizada", StringComparison.OrdinalIgnoreCase))
            { return Conflict(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpPost("sugestoes/{ocorrenciaId:guid}/ignorar")]
        public async Task<IActionResult> Ignorar(Guid ocorrenciaId)
        {
            try
            {
                await _service.IgnorarSugestaoAsync(ocorrenciaId);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("finalizada", StringComparison.OrdinalIgnoreCase))
            { return Conflict(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpGet("reserva-emergencia")]
        public async Task<IActionResult> ObterReservaEmergencia()
        {
            try { return Ok(await _service.ObterProjecaoReservaAsync()); }
            catch (Exception) { return ErroInterno(); }
        }

        private ObjectResult ErroInterno()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Não foi possível processar receitas recorrentes."
            });
        }
    }
}
