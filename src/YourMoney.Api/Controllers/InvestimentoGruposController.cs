using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Repositories;
using YourMoney.Application.Services;
using YourMoney.Api.Services;
using System.Text.RegularExpressions;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[RegraNegocioAtiva(RegraNegocioCatalogo.ObjetivosInvestimento)]
[Route("api/InvestimentoGrupos")]
public sealed class InvestimentoGruposController : ControllerBase
{
    private const long TamanhoMaximoImagem = 5 * 1024 * 1024;
    private static readonly Regex NomeImagemValido = new(@"^[0-9a-f]{32}\.(png|jpg|webp)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly HashSet<string> ImagensPadrao = new(StringComparer.Ordinal)
    {
        "/assets/objectives/city-lx-sedan.jpg",
        "/assets/objectives/financial-freedom.jpg",
        "/assets/objectives/long-term.jpg",
        "/assets/objectives/emergency-fund.jpg"
    };
    private readonly IGrupoObjetivoInvestimentoService _service;
    private readonly IWebHostEnvironment? _environment;
    private readonly ICurrentUserService? _currentUser;
    private readonly bool _uploadsImagemHabilitados;

    public InvestimentoGruposController(
        IGrupoObjetivoInvestimentoService service,
        ICurrentUserService? currentUser = null,
        IWebHostEnvironment? environment = null,
        IConfiguration? configuration = null)
    {
        _service = service;
        _currentUser = currentUser;
        _environment = environment;
        var configuracaoUploads = configuration?["OBJECTIVE_IMAGE_UPLOADS_ENABLED"];
        _uploadsImagemHabilitados = bool.TryParse(configuracaoUploads, out var habilitados)
            ? habilitados
            : environment?.IsDevelopment() == true;
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
            ValidarImagemParaSalvar(request?.ImagemUrl, null);
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
        if (!_uploadsImagemHabilitados)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                Erro("upload_imagem_indisponivel", "O envio de imagens está temporariamente indisponível neste piloto."));

        if (imagem == null || imagem.Length == 0)
            return BadRequest(Erro("imagem_obrigatoria", "Selecione uma imagem para o objetivo."));
        if (imagem.Length > TamanhoMaximoImagem)
            return BadRequest(Erro("imagem_muito_grande", "A imagem deve ter no máximo 5 MB."));

        var extensao = await DetectarExtensaoAsync(imagem);
        if (extensao == null)
            return BadRequest(Erro("imagem_invalida", "Use uma imagem PNG, JPEG ou WebP válida."));

        var pasta = PastaImagensTitular(ObterRaizAplicacao());
        Directory.CreateDirectory(pasta);

        var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
        var destino = Path.Combine(pasta, nomeArquivo);
        await using (var stream = System.IO.File.Create(destino))
            await imagem.CopyToAsync(stream, HttpContext.RequestAborted);

        return Ok(new { imagemUrl = $"/uploads/objetivos/{nomeArquivo}" });
    }

    [HttpGet("imagens/status")]
    public IActionResult ObterStatusUploadImagem() => Ok(new { uploadHabilitado = _uploadsImagemHabilitados });

    [HttpGet("imagens/{nomeArquivo}")]
    public async Task<IActionResult> ObterImagem(string nomeArquivo)
    {
        if (!NomeImagemValido.IsMatch(nomeArquivo ?? string.Empty))
            return NotFound();

        var raizAplicacao = ObterRaizAplicacao();
        var arquivoTitular = Path.Combine(PastaImagensTitular(raizAplicacao), nomeArquivo);
        if (System.IO.File.Exists(arquivoTitular))
            return ServirImagemPrivada(arquivoTitular, nomeArquivo);

        // Compatibility for images uploaded before private per-user folders existed.
        // A legacy image is served only when one of this user's saved goals references it.
        var urlLegada = $"/uploads/objetivos/{nomeArquivo}";
        var imagensTitular = await _service.ListarAsync(true);
        if (!imagensTitular.Any(grupo => string.Equals(grupo.ImagemUrl, urlLegada, StringComparison.Ordinal)))
            return NotFound();

        var arquivoLegado = Path.Combine(PastaImagensLegadas(raizAplicacao), nomeArquivo);
        if (!System.IO.File.Exists(arquivoLegado))
            return NotFound();

        return ServirImagemPrivada(arquivoLegado, nomeArquivo);
    }

    [HttpDelete("imagens/{nomeArquivo}")]
    public async Task<IActionResult> ExcluirImagemTemporaria(string nomeArquivo)
    {
        if (!NomeImagemValido.IsMatch(nomeArquivo ?? string.Empty))
            return NotFound();

        var url = $"/uploads/objetivos/{nomeArquivo}";
        var grupos = await _service.ListarAsync(true);
        if (grupos.Any(grupo => string.Equals(grupo.ImagemUrl, url, StringComparison.Ordinal)))
            return Conflict(Erro("imagem_em_uso", "A imagem está associada a um objetivo."));

        var arquivo = Path.Combine(PastaImagensTitular(ObterRaizAplicacao()), nomeArquivo);
        if (System.IO.File.Exists(arquivo))
            System.IO.File.Delete(arquivo);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Renomear(Guid id, [FromBody] RenomearGrupoObjetivoRequest request)
    {
        try
        {
            var atual = await _service.ObterAsync(id);
            ValidarImagemParaSalvar(request?.ImagemUrl, atual);
            var atualizado = await _service.RenomearAsync(id, request);
            if (!string.Equals(atual.ImagemUrl, atualizado.ImagemUrl, StringComparison.Ordinal))
                await ExcluirImagemPrivadaSeOrfaAsync(atual.ImagemUrl);
            return Ok(atualizado);
        }
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
            var atual = await _service.ObterAsync(id);
            await _service.ExcluirAsync(id, request);
            await ExcluirImagemPrivadaSeOrfaAsync(atual.ImagemUrl);
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

    private void ValidarImagemParaSalvar(string? imagemUrl, GrupoObjetivoInvestimentoResponse? objetivoAtual)
    {
        if (string.IsNullOrWhiteSpace(imagemUrl) || ImagensPadrao.Contains(imagemUrl))
            return;

        if (!TryObterNomeImagem(imagemUrl, out var nomeArquivo))
            throw new ArgumentException("A imagem do objetivo é inválida.");

        var arquivoTitular = Path.Combine(PastaImagensTitular(ObterRaizAplicacao()), nomeArquivo);
        if (System.IO.File.Exists(arquivoTitular))
            return;

        // Allows keeping an image from the legacy public directory on the same owned goal.
        if (objetivoAtual != null && string.Equals(objetivoAtual.ImagemUrl, imagemUrl, StringComparison.Ordinal))
            return;

        throw new ArgumentException("Envie novamente a imagem para associá-la a este objetivo.");
    }

    private async Task ExcluirImagemPrivadaSeOrfaAsync(string? imagemUrl)
    {
        if (!TryObterNomeImagem(imagemUrl, out var nomeArquivo))
            return;

        var grupos = await _service.ListarAsync(true);
        if (grupos.Any(grupo => string.Equals(grupo.ImagemUrl, imagemUrl, StringComparison.Ordinal)))
            return;

        var arquivo = Path.Combine(PastaImagensTitular(ObterRaizAplicacao()), nomeArquivo);
        if (!System.IO.File.Exists(arquivo))
            return;

        try { System.IO.File.Delete(arquivo); }
        catch (IOException) { /* A remoção do objetivo já foi concluída; a limpeza pode ser repetida depois. */ }
        catch (UnauthorizedAccessException) { /* A remoção do objetivo já foi concluída; a limpeza pode ser repetida depois. */ }
    }

    private PhysicalFileResult ServirImagemPrivada(string caminho, string nomeArquivo)
    {
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return PhysicalFile(caminho, ObterContentType(nomeArquivo));
    }

    private string ObterRaizAplicacao()
    {
        var raiz = _environment?.ContentRootPath;
        return string.IsNullOrWhiteSpace(raiz) ? AppContext.BaseDirectory : raiz;
    }

    private string PastaImagensTitular(string raizAplicacao)
    {
        var userId = _currentUser?.UserId
            ?? User?.FindFirstValue("sub")
            ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Usuário autenticado não identificado.");
        var hashTitular = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId)));
        return Path.Combine(raizAplicacao, "App_Data", "ObjectiveImages", hashTitular);
    }

    private static string PastaImagensLegadas(string raizAplicacao) =>
        Path.Combine(raizAplicacao, "App_Data", "ObjectiveImages");

    private static bool TryObterNomeImagem(string? imagemUrl, out string nomeArquivo)
    {
        nomeArquivo = string.Empty;
        const string prefixo = "/uploads/objetivos/";
        if (string.IsNullOrWhiteSpace(imagemUrl) || !imagemUrl.StartsWith(prefixo, StringComparison.Ordinal))
            return false;

        var candidato = imagemUrl[prefixo.Length..];
        if (!NomeImagemValido.IsMatch(candidato))
            return false;

        nomeArquivo = candidato;
        return true;
    }

    private static string ObterContentType(string nomeArquivo) =>
        Path.GetExtension(nomeArquivo).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

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
