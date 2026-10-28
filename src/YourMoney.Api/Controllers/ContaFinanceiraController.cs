using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ContaFinanceiraController : ControllerBase
{
    private static readonly string[] InstituicoesSugeridas =
    [
        "Banco do Brasil", "Bradesco", "Caixa Econômica Federal", "Itaú Unibanco", "Santander",
        "Nubank", "Banco Inter", "C6 Bank", "BTG Pactual", "Sicoob", "Sicredi",
        "Mercado Pago", "PicPay", "PagBank", "Carteira / Dinheiro"
    ];

    private readonly IContaFinanceiraService _service;

    public ContaFinanceiraController(IContaFinanceiraService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ContaFinanceiraRequest request)
    {
        try
        {
            var conta = await _service.CriarAsync(request);
            return CreatedAtAction(nameof(Obter), new { id = conta.Id }, conta);
        }
        catch (ArgumentException ex)
        {
            return Problema(StatusCodes.Status400BadRequest, "Conta inválida.", ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await _service.ListarAsync());

    [HttpGet("instituicoes")]
    public IActionResult ListarInstituicoes() => Ok(InstituicoesSugeridas);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        try
        {
            return Ok(await _service.GetByIdAsync(id));
        }
        catch (InvalidOperationException)
        {
            return Problema(StatusCodes.Status404NotFound, "Conta não encontrada.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ContaFinanceiraRequest request)
    {
        try
        {
            return Ok(await _service.AtualizarAsync(id, request));
        }
        catch (ArgumentException ex)
        {
            return Problema(StatusCodes.Status400BadRequest, "Conta inválida.", ex.Message);
        }
        catch (InvalidOperationException)
        {
            return Problema(StatusCodes.Status404NotFound, "Conta não encontrada.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        try
        {
            await _service.RemoverContaFinanceiraAsync(id);
            return NoContent();
        }
        catch (ContaFinanceiraEmUsoException ex)
        {
            return Problema(StatusCodes.Status409Conflict, "Conta em uso.", ex.Message);
        }
        catch (InvalidOperationException)
        {
            return Problema(StatusCodes.Status404NotFound, "Conta não encontrada.");
        }
    }

    private ObjectResult Problema(int status, string title, string? detail = null) =>
        StatusCode(status, new ProblemDetails { Status = status, Title = title, Detail = detail });
}
