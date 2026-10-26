using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/InvestimentoGrupos")]
public sealed class InvestimentoGruposController : ControllerBase
{
    private readonly IGrupoObjetivoInvestimentoService _service;

    public InvestimentoGruposController(IGrupoObjetivoInvestimentoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        try { return Ok(await _service.ListarAsync()); }
        catch (Exception) { return FalhaInesperada(); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        try { return Ok(await _service.ObterAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(Erro("grupo_nao_encontrado", ex.Message)); }
        catch (Exception) { return FalhaInesperada(); }
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarGrupoObjetivoRequest request)
    {
        try
        {
            var response = await _service.CriarAsync(request);
            return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
        }
        catch (ArgumentException ex) { return BadRequest(Erro("grupo_invalido", ex.Message)); }
        catch (GrupoObjetivoConflitoException ex) { return Conflict(Erro(ex.Codigo, ex.Message)); }
        catch (Exception) { return FalhaInesperada(); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Renomear(Guid id, [FromBody] RenomearGrupoObjetivoRequest request)
    {
        try { return Ok(await _service.RenomearAsync(id, request)); }
        catch (ArgumentException ex) { return BadRequest(Erro("grupo_invalido", ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(Erro("grupo_nao_encontrado", ex.Message)); }
        catch (GrupoObjetivoConflitoException ex) { return Conflict(Erro(ex.Codigo, ex.Message)); }
        catch (Exception) { return FalhaInesperada(); }
    }

    [HttpPost("{id:guid}/exclusao")]
    public async Task<IActionResult> Excluir(Guid id, [FromBody] ExcluirGrupoObjetivoRequest request)
    {
        try
        {
            await _service.ExcluirAsync(id, request);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(Erro("exclusao_invalida", ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(Erro("grupo_nao_encontrado", ex.Message)); }
        catch (GrupoObjetivoConflitoException ex) { return Conflict(Erro(ex.Codigo, ex.Message)); }
        catch (Exception) { return FalhaInesperada(); }
    }

    private static ErroApiResponse Erro(string code, string message) => new() { Code = code, Message = message };

    private ObjectResult FalhaInesperada() => StatusCode(
        StatusCodes.Status500InternalServerError,
        Erro("erro_interno", "Não foi possível processar os objetivos financeiros."));
}
