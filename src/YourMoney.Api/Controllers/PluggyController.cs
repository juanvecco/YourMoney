using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourMoney.Api.Services;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/OpenFinance")]
public sealed class PluggyController : ControllerBase
{
    private readonly IPluggyService _pluggyService;
    private readonly ICurrentUserService _currentUserService;

    public PluggyController(
        IPluggyService pluggyService,
        ICurrentUserService currentUserService)
    {
        _pluggyService = pluggyService;
        _currentUserService = currentUserService;
    }

    [HttpGet("pluggy-configuration")]
    public async Task<IActionResult> ObterConfiguracaoCredenciais(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _pluggyService.ObterConfiguracaoCredenciaisAsync(
                _currentUserService.UserId,
                cancellationToken));
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    [HttpPut("pluggy-configuration")]
    public async Task<IActionResult> SalvarConfiguracaoCredenciais(
        [FromBody] PluggyCredentialRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _pluggyService.SalvarConfiguracaoCredenciaisAsync(
                _currentUserService.UserId,
                request,
                cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PluggyCredentialsInvalidException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PluggyCredentialsManagedExternallyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Não foi possível validar as credenciais com a Pluggy agora. Tente novamente."
            });
        }
    }

    [HttpPost("connect-token")]
    public async Task<IActionResult> CriarConnectToken(
        [FromBody] PluggyConnectTokenRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _pluggyService.CriarConnectTokenAsync(
                _currentUserService.UserId,
                request?.ConnectionId,
                cancellationToken);
            return Ok(response);
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConnectionLimitException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Nao foi possivel comunicar com a Pluggy agora. Tente novamente."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("connections")]
    public async Task<IActionResult> RegistrarConexao(
        [FromBody] PluggyConnectionRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var connection = await _pluggyService.RegistrarConexaoAsync(
                _currentUserService.UserId,
                request?.ItemId ?? string.Empty,
                request?.ConnectionId,
                cancellationToken);
            return Ok(connection);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConnectionLimitException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Conexão bancária não encontrada para este usuário." });
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível comunicar com a Pluggy agora." });
        }
    }

    [HttpGet("connections")]
    public async Task<IActionResult> ListarConexoes(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _pluggyService.ListarConexoesAsync(_currentUserService.UserId, cancellationToken));
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível comunicar com a Pluggy agora." });
        }
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> ListarContas(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _pluggyService.ListarContasAsync(_currentUserService.UserId, cancellationToken));
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConsentExpiredException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível comunicar com a Pluggy agora." });
        }
    }

    [HttpGet("real-transactions/preview")]
    public async Task<IActionResult> ListarTransacoesPreview(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _pluggyService.ListarTransacoesPreviewAsync(_currentUserService.UserId, cancellationToken));
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConsentExpiredException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível comunicar com a Pluggy agora." });
        }
    }

    [HttpGet("real-transactions/overview")]
    public async Task<IActionResult> ObterVisaoGeralTransacoes(
        [FromQuery] Guid? connectionId,
        [FromQuery] string? accountId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _pluggyService.ObterVisaoGeralAsync(
                _currentUserService.UserId,
                new PluggyOverviewFilterDTO
                {
                    ConnectionId = connectionId,
                    AccountId = accountId,
                    From = from,
                    To = to
                },
                cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConsentExpiredException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível comunicar com a Pluggy agora." });
        }
    }

    [HttpPost("real-transactions/import")]
    public async Task<IActionResult> ImportarTransacao(
        [FromBody] PluggyTransactionImportRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pluggyService.ImportarTransacaoAsync(
                _currentUserService.UserId,
                request,
                cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PluggyTransactionAlreadyRegisteredException ex)
        {
            return Conflict(new
            {
                code = "already_registered",
                message = ex.Message,
                existingImport = ex.ExistingImport
            });
        }
        catch (PluggyTransactionNotImportableException ex)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyConsentExpiredException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível comunicar com a Pluggy agora." });
        }
    }

    [HttpPost("real-transactions/reconcile")]
    public async Task<IActionResult> ReconciliarDespesa(
        [FromBody] PluggyExpenseReconciliationRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pluggyService.ReconciliarDespesaAsync(
                _currentUserService.UserId,
                request,
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "validation_error", message = ex.Message });
        }
        catch (PluggyTransactionAlreadyRegisteredException ex)
        {
            return Conflict(new
            {
                code = "already_registered",
                message = ex.Message,
                existingImport = ex.ExistingImport
            });
        }
        catch (PluggyExpenseReconciliationConflictException ex)
        {
            return Conflict(new { code = ex.Code, message = ex.Message });
        }
        catch (PluggyTransactionNotImportableException ex)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity,
                new { code = "unsupported_transaction", message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { code = "not_found", message = ex.Message });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { code = "forbidden", message = ex.Message });
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { code = "provider_unavailable", message = ex.Message });
        }
        catch (PluggyConsentExpiredException ex)
        {
            return Conflict(new { code = "consent_expired", message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new { code = "provider_unavailable", message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                new { code = "provider_unavailable", message = "Não foi possível comunicar com a Pluggy agora." });
        }
        catch (DbUpdateException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { code = "persistence_error", message = "Não foi possível salvar a reconciliação. Revise os dados e tente novamente." });
        }
    }

    [HttpDelete("connections/{id:guid}")]
    public async Task<IActionResult> RemoverConexao(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _pluggyService.RemoverConexaoAsync(_currentUserService.UserId, id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Conexão bancária não encontrada para este usuário." });
        }
        catch (PluggyAuthorizationException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PluggyConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (PluggyIntegrationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível comunicar com a Pluggy agora." });
        }
    }
}
