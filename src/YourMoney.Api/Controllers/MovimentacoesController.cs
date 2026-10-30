using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class MovimentacoesController : ControllerBase
{
    private readonly IMovimentacaoFinanceiraService _service;

    public MovimentacoesController(IMovimentacaoFinanceiraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Consultar([FromQuery] ConsultaMovimentacoesRequest request)
    {
        try
        {
            return Ok(await _service.ConsultarAsync(request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Não foi possível consultar as movimentações.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Não foi possível consultar as movimentações agora.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
