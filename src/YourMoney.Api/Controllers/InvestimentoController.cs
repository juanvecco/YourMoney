using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

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
                return CreatedAtAction(
                    nameof(ObterPorReferencia),
                    new
                    {
                        mes = response.DataInvestimento.Month,
                        ano = response.DataInvestimento.Year
                    },
                    response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    title = "Não foi possível salvar o investimento.",
                    status = StatusCodes.Status500InternalServerError
                });
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
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarInvestimentos()
        {
            try
            {
                return Ok(await _investimentoService.ListarAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarInvestimento(Guid id, [FromBody] InvestimentoDto dto)
        {
            var investimento = await _investimentoService.GetInvestimentoByIdAsync(id);

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                investimento.AtualizarNome(dto.Nome);

            investimento.AtualizarDescricao(dto.Descricao ?? string.Empty);
            investimento.AtualizarValorAtual(dto.ValorAtual);
            investimento.AtualizarData(dto.DataInvestimento);

            if (!string.IsNullOrWhiteSpace(dto.Tipo))
                investimento.AtualizarTipo(dto.Tipo);
            if (dto.Quantidade > 0)
                investimento.AtualizarQuantidade(dto.Quantidade);
            if (dto.PrecoMedio > 0)
                investimento.AtualizarPrecoMedio(dto.PrecoMedio);

            await _investimentoService.AtualizarAsync(investimento);
            return NoContent();
        }

        [HttpGet("por-referencia")]
        public async Task<IActionResult> ObterPorReferencia([FromQuery] int mes, [FromQuery] int ano)
        {
            return Ok(await _investimentoService.ObterPorMesAnoAsync(mes, ano));
        }
    }
}
