using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CarteirasController : ControllerBase
{
    private readonly ICarteiraService _service;

    public CarteirasController(ICarteiraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await _service.ListarAsync());

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CarteiraRequestDTO request)
    {
        try
        {
            return StatusCode(StatusCodes.Status201Created, await _service.CriarAsync(request));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Carteira inválida.", Detail = exception.Message });
        }
        catch (CarteiraNomeDuplicadoException exception)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Nome de carteira repetido.", Detail = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Renomear(Guid id, [FromBody] CarteiraRequestDTO request)
    {
        try
        {
            return Ok(await _service.RenomearAsync(id, request));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Carteira inválida.", Detail = exception.Message });
        }
        catch (CarteiraNaoEncontradaException exception)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Carteira não encontrada.", Detail = exception.Message });
        }
        catch (CarteiraNomeDuplicadoException exception)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Nome de carteira repetido.", Detail = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        try
        {
            await _service.RemoverAsync(id);
            return NoContent();
        }
        catch (CarteiraNaoEncontradaException exception)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Carteira não encontrada.", Detail = exception.Message });
        }
        catch (CarteiraPrincipalException exception)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Carteira principal.", Detail = exception.Message });
        }
        catch (CarteiraComDependenciasException exception)
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Carteira com dados associados.", Detail = exception.Message });
        }
    }
}
