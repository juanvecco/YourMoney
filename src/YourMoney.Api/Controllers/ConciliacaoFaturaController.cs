using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ConciliacaoFaturaController : ControllerBase
{
    private readonly IConciliacaoFaturaService _service;
    public ConciliacaoFaturaController(IConciliacaoFaturaService service) => _service = service;

    [HttpGet("{contaId:guid}")]
    public async Task<IActionResult> Obter(Guid contaId, [FromQuery] int mes, [FromQuery] int ano)
    {
        try { return Ok(await _service.ObterAsync(contaId, mes, ano)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException) { return NotFound(new { message = "Conta não encontrada." }); }
    }

    [HttpPut("{contaId:guid}")]
    public async Task<IActionResult> Salvar(Guid contaId, [FromBody] SalvarFaturaRequest request)
    {
        if (request is null) return BadRequest(new { message = "Dados da fatura são obrigatórios." });
        try { return Ok(await _service.SalvarAsync(contaId, request.Mes, request.Ano, request.ValorInformado)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException) { return NotFound(new { message = "Conta não encontrada." }); }
    }
}
