using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Api.Services;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Authorize]
    [RegraNegocioAtiva(RegraNegocioCatalogo.ReceitasRecorrentes)]
    [Route("api/[controller]")]
    public class ReceitasRecorrentesController : ControllerBase
    {
        private readonly IReceitaRecorrenteService _service;
        private readonly IRegraNegocioService _regras;

        public ReceitasRecorrentesController(IReceitaRecorrenteService service, IRegraNegocioService regras = null)
        {
            _service = service;
            _regras = regras;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] ReceitaRecorrenteRequest request)
        {
            try
            {
                var bloqueio = await VerificarMarcacoesAsync(request, null);
                if (bloqueio is not null) return bloqueio;
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
            try
            {
                var atual = await _service.ObterPorIdAsync(id);
                var bloqueio = await VerificarMarcacoesAsync(request, atual);
                if (bloqueio is not null) return bloqueio;
                return Ok(await _service.AtualizarAsync(id, request));
            }
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
        [RegraNegocioAtiva(RegraNegocioCatalogo.ReservaEmergencia)]
        public async Task<IActionResult> ObterReservaEmergencia()
        {
            try { return Ok(await _service.ObterProjecaoReservaAsync()); }
            catch (Exception) { return ErroInterno(); }
        }

        [HttpGet("elegiveis-para-investimento")]
        [RegraNegocioAtiva(RegraNegocioCatalogo.RendaSalarial)]
        public async Task<IActionResult> ListarElegiveisParaInvestimento()
        {
            try { return Ok(await _service.ListarElegiveisParaInvestimentoAsync()); }
            catch (Exception) { return ErroInterno(); }
        }

        private ObjectResult ErroInterno()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Não foi possível processar receitas recorrentes."
            });
        }

        private async Task<IActionResult> VerificarMarcacoesAsync(ReceitaRecorrenteRequest request, ReceitaRecorrenteResponse atual)
        {
            if (_regras is null || request is null) return null;
            if (request.EhSalario && atual?.EhSalario != true
                && !await _regras.EstaAtivaAsync(RegraNegocioCatalogo.RendaSalarial))
                return StatusCode(403, new { message = "Investimentos por renda salarial estão desativados." });
            if (request.ConsideraReservaEmergencia && atual?.ConsideraReservaEmergencia != true
                && !await _regras.EstaAtivaAsync(RegraNegocioCatalogo.ReservaEmergencia))
                return StatusCode(403, new { message = "Reserva de emergência está desativada." });
            return null;
        }
    }
}
