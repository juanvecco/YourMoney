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
    public class MetasController : ControllerBase
    {
        private readonly IMetaMensalService _metaMensalService;
        private readonly IAporteObjetivoInvestimentoService? _aporteService;

        public MetasController(IMetaMensalService metaMensalService, IAporteObjetivoInvestimentoService? aporteService = null)
        {
            _metaMensalService = metaMensalService;
            _aporteService = aporteService;
        }

        [HttpGet("{id:guid}/aportes")]
        public async Task<IActionResult> ListarAportes(Guid id)
        {
            try { return Ok(await ObterAporteService().ListarAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new ErroApiResponse { Code = "meta_nao_encontrada", Message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new ErroApiResponse { Code = "erro_interno", Message = "Não foi possível carregar os aportes." }); }
        }

        [HttpPost("{id:guid}/aportes")]
        public async Task<IActionResult> ConfirmarAporte(Guid id, [FromBody] ConfirmarAporteObjetivoRequest request)
        {
            try
            {
                var aporte = await ObterAporteService().ConfirmarAsync(id, request);
                return aporte.CriadoAgora ? StatusCode(StatusCodes.Status201Created, aporte) : Ok(aporte);
            }
            catch (ArgumentException ex) { return BadRequest(new ErroApiResponse { Code = "aporte_invalido", Message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new ErroApiResponse { Code = "recurso_nao_encontrado", Message = ex.Message }); }
            catch (ConflitoAporteObjetivoException ex) { return Conflict(new ErroApiResponse { Code = ex.Codigo, Message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new ErroApiResponse { Code = "erro_interno", Message = "Não foi possível confirmar o aporte." }); }
        }

        [HttpPut("{metaId:guid}/aportes/{aporteId:guid}/alocacao")]
        public async Task<IActionResult> AlocarAporte(Guid metaId, Guid aporteId, [FromBody] AlocarAporteObjetivoRequest request)
        {
            try { return Ok(await ObterAporteService().AlocarAsync(metaId, aporteId, request)); }
            catch (ArgumentException ex) { return BadRequest(new ErroApiResponse { Code = "alocacao_invalida", Message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new ErroApiResponse { Code = "recurso_nao_encontrado", Message = ex.Message }); }
            catch (ConflitoAporteObjetivoException ex) { return Conflict(new ErroApiResponse { Code = ex.Codigo, Message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new ErroApiResponse { Code = "erro_interno", Message = "Não foi possível alocar o aporte." }); }
        }

        private IAporteObjetivoInvestimentoService ObterAporteService() =>
            _aporteService ?? throw new InvalidOperationException("Serviço de aportes não configurado.");

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
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível carregar as metas." });
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
            catch (ConflitoMetaMensalException ex)
            {
                return Conflict(new { message = ex.Message });
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
            catch (ConflitoMetaMensalException ex)
            {
                return Conflict(new { message = ex.Message });
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
