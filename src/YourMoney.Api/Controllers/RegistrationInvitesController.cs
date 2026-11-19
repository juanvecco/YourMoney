using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Services;

namespace YourMoney.Api.Controllers;

[Route("api/identidade/convites")]
public sealed class RegistrationInvitesController : ControllerBase
{
    private readonly RegistrationInviteService _invites;
    private readonly IConfiguration _configuration;

    public RegistrationInvitesController(RegistrationInviteService invites, IConfiguration configuration)
    {
        _invites = invites;
        _configuration = configuration;
    }

    [HttpGet("validar")]
    [AllowAnonymous]
    public async Task<ActionResult<InviteCheck>> Validate([FromQuery] string codigo, CancellationToken cancellationToken) =>
        Ok(await _invites.CheckAsync(codigo, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InviteIssued>> Issue(CancellationToken cancellationToken)
    {
        var adminId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(adminId)) return Unauthorized();
        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/')
            ?? $"{(_configuration.GetValue<bool>("TLS_TERMINATED_BY_PROXY") ? "https" : Request.Scheme)}://{Request.Host}";
        return Ok(await _invites.IssueAsync(adminId, baseUrl, cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<InviteSummary>>> List(CancellationToken cancellationToken)
    {
        var adminId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(adminId)) return Unauthorized();
        return Ok(await _invites.ListAsync(adminId, cancellationToken));
    }

    [HttpPost("{id}/revogar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Revoke(string id, CancellationToken cancellationToken)
    {
        var adminId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(adminId)) return Unauthorized();
        if (id.Length != 64 || id.Any(ch => !Uri.IsHexDigit(ch))) return BadRequest();
        return await _invites.RevokeAsync(adminId, id, cancellationToken) ? NoContent() : NotFound();
    }
}
