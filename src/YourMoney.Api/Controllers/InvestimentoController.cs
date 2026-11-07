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
        private readonly IRegraNegocioService _regras;

        public InvestimentoController(IInvestimentoService investimentoService, IRegraNegocioService regras = null)
        {
            _investimentoService = investimentoService;
            _regras = regras;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarInvestimento([FromBody] CriarInvestimentoRequest request)
        {
            try
            {
                var bloqueio = await VerificarAssociacoesAsync(request, null);
                if (bloqueio is not null) return bloqueio;
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
                return BadRequest(Erro("investimento_invalido", ex.Message));
            }
            catch (ConflitoOperacaoInvestimentoException ex)
            {
                return Conflict(new ErroApiResponse { Code = "operacao_conflitante", Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErroApiResponse { Code = "grupo_objetivo_indisponivel", Message = ex.Message });
            }
            catch (Exception)
            {
                return FalhaInesperada();
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoverInvestimento(Guid id)
        {
            try
            {
                await _investimentoService.RemoverInvestimentoAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Erro("investimento_nao_encontrado", ex.Message));
            }
            catch (Exception) { return FalhaInesperada(); }
        }

        [HttpGet]
        public async Task<IActionResult> ListarInvestimentos()
        {
            try { return Ok(await _investimentoService.ListarAsync()); }
            catch (Exception) { return FalhaInesperada(); }
        }

        [HttpGet("consolidado")]
        public async Task<IActionResult> ObterConsolidado(
            [FromQuery] Guid? grupoObjetivoId = null,
            [FromQuery] bool semGrupo = false,
            [FromQuery] string tipoAtivo = null,
            [FromQuery] bool naoClassificado = false)
        {
            try
            {
                return Ok(await _investimentoService.ObterConsolidadoAsync(new FiltroOrganizacaoInvestimentos
                {
                    GrupoObjetivoId = grupoObjetivoId,
                    SemGrupo = semGrupo,
                    TipoAtivo = tipoAtivo,
                    NaoClassificado = naoClassificado
                }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(Erro("filtro_invalido", ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Erro("grupo_objetivo_indisponivel", ex.Message));
            }
            catch (Exception) { return FalhaInesperada(); }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            try { return Ok(await _investimentoService.ObterPorIdAsync(id)); }
            catch (InvalidOperationException ex) { return NotFound(Erro("investimento_nao_encontrado", ex.Message)); }
            catch (Exception) { return FalhaInesperada(); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> AtualizarInvestimento(
            Guid id,
            [FromBody] AtualizarInvestimentoRequest request)
        {
            try
            {
                var atual = await _investimentoService.ObterPorIdAsync(id);
                var bloqueio = await VerificarAssociacoesAsync(request, atual);
                if (bloqueio is not null) return bloqueio;
                return Ok(await _investimentoService.AtualizarInvestimentoAsync(id, request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(Erro("investimento_invalido", ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(Erro("investimento_nao_encontrado", ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErroApiResponse { Code = "grupo_objetivo_indisponivel", Message = ex.Message });
            }
            catch (Exception)
            {
                return FalhaInesperada();
            }
        }

        [HttpGet("por-referencia")]
        public async Task<IActionResult> ObterPorReferencia([FromQuery] int mes, [FromQuery] int ano)
        {
            if (mes is < 1 or > 12 || ano is < 1900 or > 9999)
                return BadRequest(Erro("referencia_invalida", "Informe um mês e ano de referência válidos."));
            try { return Ok(await _investimentoService.ObterPorMesAnoAsync(mes, ano)); }
            catch (Exception) { return FalhaInesperada(); }
        }

        private static ErroApiResponse Erro(string code, string message) => new() { Code = code, Message = message };

        private async Task<IActionResult> VerificarAssociacoesAsync(InvestimentoWriteRequest request, InvestimentoResponse atual)
        {
            if (_regras is null || request is null) return null;
            if (request.ReceitaRecorrenteId.HasValue
                && request.ReceitaRecorrenteId != atual?.ReceitaRecorrenteId
                && !await _regras.EstaAtivaAsync(RegraNegocioCatalogo.RendaSalarial))
                return StatusCode(403, new { message = "Investimentos por renda salarial estão desativados." });
            if (request.GrupoObjetivoId.HasValue
                && request.GrupoObjetivoId != atual?.GrupoObjetivoId
                && !await _regras.EstaAtivaAsync(RegraNegocioCatalogo.ObjetivosInvestimento))
                return StatusCode(403, new { message = "Objetivos de investimento estão desativados." });
            return null;
        }

        private ObjectResult FalhaInesperada()
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErroApiResponse
                {
                    Code = "erro_interno",
                    Message = "Não foi possível processar os investimentos.",
                    Title = "Não foi possível processar os investimentos.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
}
