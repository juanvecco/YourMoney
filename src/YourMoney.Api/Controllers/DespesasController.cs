using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DespesasController : ControllerBase
    {
        private readonly IDespesaService _despesaService;

        public DespesasController(IDespesaService despesaService)
        {
            _despesaService = despesaService;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarDespesa([FromBody] CriarDespesaRequest request)
        {
            try
            {
                var response = await _despesaService.CriarDespesaAsync(request);
                return CreatedAtAction(nameof(GetDespesaById), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDespesaById(Guid id)
        {
            try
            {
                var despesa = await _despesaService.ObterDtoPorIdAsync(id);
                return Ok(despesa);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("parcelamento")]
        [Authorize]
        public async Task<IActionResult> CriarParcelamento([FromBody] ParcelamentoDespesaRequest request)
        {
            try
            {
                var response = await _despesaService.CriarParcelamentoAsync(request);
                var primeiraParcela = response.Parcelas.FirstOrDefault();

                return CreatedAtAction(
                    nameof(ObterPorReferencia),
                    new
                    {
                        mes = primeiraParcela?.Data.Month,
                        ano = primeiraParcela?.Data.Year,
                        idContaFinanceira = primeiraParcela?.IdContaFinanceira
                    },
                    response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverDespesa(Guid id)
        {
            try
            {
                await _despesaService.RemoverDespesaAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarDespesas()
        {
            try
            {
                var despesas = await _despesaService.ListarAsync();
                return Ok(despesas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("consulta")]
        public async Task<IActionResult> ConsultarDespesas([FromQuery] ConsultaDespesasRequest request)
        {
            try
            {
                var despesas = await _despesaService.ConsultarDespesasAsync(request);
                return Ok(despesas);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "Filtros de despesa inválidos." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível consultar despesas." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarDespesa(Guid id, [FromBody] DespesaDTO dto)
        {
            var despesa = await _despesaService.GetDespesaByIdAsync(id);

            despesa.AtualizarDescricao(dto.Descricao);
            despesa.AtualizarValor(dto.Valor);
            despesa.AtualizarData(dto.Data);
            despesa.AtualizarContaFinanceira(dto.IdContaFinanceira);
            despesa.AtualizarCategoria(dto.IdCategoria);
            await _despesaService.AtualizarAsync(despesa);
            return NoContent();
        }

        [HttpGet("por-referencia")]
        public async Task<IActionResult> ObterPorReferencia([FromQuery] int mes, [FromQuery] int ano, [FromQuery] Guid? idContaFinanceira)
        {
            var despesas = await _despesaService.ObterPorMesAnoAsync(mes, ano, idContaFinanceira);
            return Ok(despesas);
        }
    }
}
