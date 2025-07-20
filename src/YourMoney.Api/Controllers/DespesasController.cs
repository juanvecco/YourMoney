using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DespesasController : ControllerBase
    {
        private readonly IDespesaService _despesaService;

        public DespesasController(IDespesaService despesaService)
        {
            _despesaService = despesaService;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarDespesa([FromBody] Despesa despesa)
        {
            try
            {
                await _despesaService.AdicionarDespesaAsync(despesa);
                return CreatedAtAction(nameof(AdicionarDespesa), new { id = despesa.Id }, despesa);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarDespesa(Guid id, [FromBody] DespesaDTO dto)
        {
            var despesa = await _despesaService.GetDespesaByIdAsync(id);

            despesa.AtualizarDescricao(dto.Descricao);
            despesa.AtualizarValor(dto.Valor);
            despesa.AtualizarData(dto.Data);
            despesa.AtualizarContaFinanceira(dto.IdContaFinanceira);

            await _despesaService.AtualizarAsync(despesa);
            return NoContent();
        }
        //[HttpPut("{id}/pagar")]
        //public async Task<IActionResult> MarcarComoPaga(Guid id)
        //{
        //    try
        //    {
        //        var despesa = await _despesaService.GetDespesaByIdAsync(id);
        //        despesa.MarcarComoPaga();
        //        await _despesaService.AtualizarAsync(despesa);
        //        return NoContent();
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        //[HttpPut("{id}/desmarcar-pagamento")]
        //public async Task<IActionResult> DesmarcarPagamento(Guid id)
        //{
        //    try
        //    {
        //        var despesa = await _despesaService.GetDespesaByIdAsync(id);
        //        despesa.DesmarcarPagamento();
        //        await _despesaService.AtualizarAsync(despesa);
        //        return NoContent();
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return NotFound(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}