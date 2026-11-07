using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/regras-negocio")]
public sealed class RegrasNegocioController : ControllerBase
{
    private readonly IRegraNegocioService _rules;

    public RegrasNegocioController(IRegraNegocioService rules) => _rules = rules;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        try { return Ok(await _rules.ListarAsync()); }
        catch (Exception) { return StatusCode(500, new { message = "Não foi possível carregar as regras." }); }
    }

    [HttpPut("{codigo}")]
    public async Task<IActionResult> Atualizar(string codigo, [FromBody] AtualizarRegraNegocioRequest request)
    {
        if (request?.Ativa is null) return BadRequest(new { message = "Informe o estado da regra." });
        try { return Ok(await _rules.AtualizarAsync(codigo, request.Ativa.Value)); }
        catch (KeyNotFoundException) { return NotFound(new { message = "Regra de negócio não encontrada." }); }
        catch (Exception) { return StatusCode(500, new { message = "Não foi possível salvar a regra." }); }
    }
}

public sealed class AtualizarRegraNegocioRequest
{
    [Required]
    public bool? Ativa { get; set; }
}
