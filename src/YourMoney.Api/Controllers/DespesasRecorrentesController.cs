using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DespesasRecorrentesController : ControllerBase
    {
        private readonly IDespesaRecorrenteService _service;

        public DespesasRecorrentesController(IDespesaRecorrenteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool? ativas)
        {
            try
            {
                return Ok(await _service.ListarAsync(ativas));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível processar despesas recorrentes." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            try
            {
                return Ok(await _service.ObterPorIdAsync(id));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] DespesaRecorrenteRequest request)
        {
            try
            {
                var response = await _service.CriarAsync(request);
                return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível processar despesas recorrentes." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] DespesaRecorrenteRequest request)
        {
            try
            {
                return Ok(await _service.AtualizarAsync(id, request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/desativar")]
        public async Task<IActionResult> Desativar(Guid id)
        {
            try
            {
                await _service.DesativarAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/encerrar")]
        public async Task<IActionResult> Encerrar(Guid id, [FromBody] EncerrarDespesaRecorrenteRequest request)
        {
            try
            {
                return Ok(await _service.EncerrarAsync(id, request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("sugestoes")]
        public async Task<IActionResult> ListarSugestoes([FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                return Ok(await _service.ListarSugestoesAsync(mes, ano));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível processar despesas recorrentes." });
            }
        }

        [HttpPost("sugestoes/{ocorrenciaId}/confirmar")]
        public async Task<IActionResult> ConfirmarSugestao(Guid ocorrenciaId, [FromBody] ConfirmarSugestaoDespesaRecorrenteRequest request)
        {
            try
            {
                var response = await _service.ConfirmarSugestaoAsync(ocorrenciaId, request ?? new ConfirmarSugestaoDespesaRecorrenteRequest());
                return CreatedAtAction(nameof(DespesasController.GetDespesaById), "Despesas", new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("finalizada", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("sugestoes/{ocorrenciaId}/ignorar")]
        public async Task<IActionResult> IgnorarSugestao(Guid ocorrenciaId)
        {
            try
            {
                await _service.IgnorarSugestaoAsync(ocorrenciaId);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("finalizada", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
