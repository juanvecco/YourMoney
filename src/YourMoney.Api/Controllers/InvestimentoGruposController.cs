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
    private const long TamanhoMaximoImagem = 5 * 1024 * 1024;
    private readonly IGrupoObjetivoInvestimentoService _service;
    private readonly IWebHostEnvironment? _environment;

    public InvestimentoGruposController(IGrupoObjetivoInvestimentoService service, IWebHostEnvironment? environment = null)
    {
        _service = service;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirArquivados = false)
    {
        try { return Ok(await _service.ListarAsync(incluirArquivados)); }
        catch (Exception) { return FalhaInesperada(); }
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo([FromQuery] int mes, [FromQuery] int ano, [FromQuery] bool incluirArquivados = false)
    {
        try { return Ok(await _service.ObterResumoAsync(mes, ano, incluirArquivados)); }
        catch (ArgumentException ex) { return BadRequest(Erro("periodo_invalido", ex.Message)); }
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

    [HttpPost("imagens")]
    [RequestSizeLimit(TamanhoMaximoImagem + 64 * 1024)]
    public async Task<IActionResult> EnviarImagem([FromForm] IFormFile? imagem)
    {
        if (imagem == null || imagem.Length == 0)
            return BadRequest(Erro("imagem_obrigatoria", "Selecione uma imagem para o objetivo."));
        if (imagem.Length > TamanhoMaximoImagem)
            return BadRequest(Erro("imagem_muito_grande", "A imagem deve ter no máximo 5 MB."));

        var extensao = await DetectarExtensaoAsync(imagem);
        if (extensao == null)
            return BadRequest(Erro("imagem_invalida", "Use uma imagem PNG, JPEG ou WebP válida."));

        var raizAplicacao = _environment?.ContentRootPath;
        if (string.IsNullOrWhiteSpace(raizAplicacao))
            raizAplicacao = AppContext.BaseDirectory;
        var pasta = Path.Combine(raizAplicacao, "App_Data", "ObjectiveImages");
        Directory.CreateDirectory(pasta);

        var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
        var destino = Path.Combine(pasta, nomeArquivo);
        await using (var stream = System.IO.File.Create(destino))
            await imagem.CopyToAsync(stream, HttpContext.RequestAborted);

        return Ok(new { imagemUrl = $"/uploads/objetivos/{nomeArquivo}" });
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

    [HttpPost("{id:guid}/arquivamento")]
    public async Task<IActionResult> Arquivar(Guid id, [FromBody] VersaoGrupoObjetivoRequest request) =>
        await AlterarEstado(id, request, false);

    [HttpPost("{id:guid}/reativacao")]
    public async Task<IActionResult> Reativar(Guid id, [FromBody] VersaoGrupoObjetivoRequest request) =>
        await AlterarEstado(id, request, true);

    private async Task<IActionResult> AlterarEstado(Guid id, VersaoGrupoObjetivoRequest request, bool reativar)
    {
        try { return Ok(reativar ? await _service.ReativarAsync(id, request) : await _service.ArquivarAsync(id, request)); }
        catch (ArgumentException ex) { return BadRequest(Erro("grupo_invalido", ex.Message)); }
        catch (KeyNotFoundException ex) { return NotFound(Erro("grupo_nao_encontrado", ex.Message)); }
        catch (GrupoObjetivoConflitoException ex) { return Conflict(Erro(ex.Codigo, ex.Message)); }
        catch (Exception) { return FalhaInesperada(); }
    }

    private static ErroApiResponse Erro(string code, string message) => new() { Code = code, Message = message };

    private static async Task<string?> DetectarExtensaoAsync(IFormFile imagem)
    {
        var cabecalho = new byte[12];
        await using var stream = imagem.OpenReadStream();
        var lidos = await stream.ReadAsync(cabecalho.AsMemory(0, cabecalho.Length));

        if (lidos >= 8 && cabecalho.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ".png";
        if (lidos >= 3 && cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF)
            return ".jpg";
        if (lidos >= 12 && cabecalho.AsSpan(0, 4).SequenceEqual("RIFF"u8) && cabecalho.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return ".webp";
        return null;
    }

    private ObjectResult FalhaInesperada() => StatusCode(
        StatusCodes.Status500InternalServerError,
        Erro("erro_interno", "Não foi possível processar os objetivos financeiros."));
}
