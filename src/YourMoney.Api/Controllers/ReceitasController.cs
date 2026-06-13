using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [Route("api/[controller]")]
    public class ReceitasController : ControllerBase
    {
        private readonly IReceitaService _receitaService;

        public ReceitasController(IReceitaService receitaService)
        {
            _receitaService = receitaService;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarReceita([FromBody] CriarReceitaRequest request)
        {
            try
            {
                var response = await _receitaService.CriarReceitaAsync(request);
                return CreatedAtAction(
                    nameof(ObterPorReferencia),
                    new { mes = response.MesReferencia.Month, ano = response.MesReferencia.Year },
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
                    title = "Não foi possível salvar a receita.",
                    status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverReceita(Guid id)
        {
            try
            {
                await _receitaService.RemoverReceitaAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarReceitas()
        {
            try
            {
                var receitas = await _receitaService.ListarAsync();
                return Ok(receitas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarReceita(Guid id, [FromBody] ReceitaDTO dto)
        {
            var receita = await _receitaService.GetReceitaByIdAsync(id);

            receita.AtualizarDescricao(dto.Descricao);
            receita.AtualizarValor(dto.Valor);
            receita.AtualizarData(dto.Data);
            if (dto.MesReferencia.HasValue)
                receita.AtualizarMesReferencia(dto.MesReferencia.Value);
            await _receitaService.AtualizarAsync(receita);
            return NoContent();
        }

        [HttpGet("por-referencia")]
        public async Task<IActionResult> ObterPorReferencia([FromQuery] int mes, [FromQuery] int ano)
        {
            var receitas = await _receitaService.ObterPorMesAnoAsync(mes, ano);
            return Ok(receitas);
        }
    }
}
