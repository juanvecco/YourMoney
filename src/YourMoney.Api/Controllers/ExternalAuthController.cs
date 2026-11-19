using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourMoney.Infrastructure.Data;
using YourMoney.Api.Configuration;
using YourMoney.Api.Models;
using YourMoney.Api.Services;

namespace YourMoney.Api.Controllers;

[Route("api/identidade")]
public sealed class ExternalAuthController : MainController
{
    private const string TicketCookie = "ym_external_ticket";
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly UserManager<IdentityUser> _users;
    private readonly IConfiguration _configuration;
    private readonly ITimeLimitedDataProtector _ticketProtector;
    private readonly JwtTokenService _jwtTokens;
    private readonly IUsuarioDemonstracaoService _initialData;
    private readonly ILogger<ExternalAuthController> _logger;
    private readonly RegistrationInviteService _invites;
    private readonly ApplicationDbContext _db;

    public ExternalAuthController(
        SignInManager<IdentityUser> signIn,
        UserManager<IdentityUser> users,
        IConfiguration configuration,
        IDataProtectionProvider dataProtection,
        JwtTokenService jwtTokens,
        IUsuarioDemonstracaoService initialData,
        ILogger<ExternalAuthController> logger,
        RegistrationInviteService invites,
        ApplicationDbContext db)
    {
        _signIn = signIn;
        _users = users;
        _configuration = configuration;
        _ticketProtector = dataProtection.CreateProtector("YourMoney.ExternalLoginTicket.v1").ToTimeLimitedDataProtector();
        _jwtTokens = jwtTokens;
        _initialData = initialData;
        _logger = logger;
        _invites = invites;
        _db = db;
    }

    [HttpGet("metodos")]
    public ActionResult Methods() => Ok(new
    {
        google = ExternalProviderConfiguration.GoogleAvailable(_configuration),
        apple = ExternalProviderConfiguration.AppleAvailable(_configuration)
    });

    [HttpPost("externo/iniciar/{provider}")]
    public async Task<IActionResult> Start(string provider, [FromForm] CadastroExternoInicio request)
    {
        provider = provider?.ToLowerInvariant();
        if (provider is not ("google" or "apple")) return NotFound();
        if ((provider == "google" && !ExternalProviderConfiguration.GoogleAvailable(_configuration))
            || (provider == "apple" && !ExternalProviderConfiguration.AppleAvailable(_configuration)))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensagem = "Este método de acesso ainda não está disponível." });
        if (!ModelState.IsValid) return BadRequest(new { mensagem = "Revise seu nome e suas respostas antes de continuar." });
        if (!request.ModoEntrar && (string.IsNullOrWhiteSpace(request.Nome)
            || string.IsNullOrWhiteSpace(request.ObjetivoInicial)))
            return BadRequest(new { mensagem = "Revise seu nome e suas respostas antes de continuar." });
        if (!string.IsNullOrWhiteSpace(request.CodigoConvite)
            && !(await _invites.CheckAsync(request.CodigoConvite.Trim(), HttpContext.RequestAborted)).Valido)
            return Redirect("/cadastro?externo=erro");

        var properties = _signIn.ConfigureExternalAuthenticationProperties(provider, Url.Action(nameof(Return))!);
        properties.Items["onboarding_name"] = request.Nome?.Trim() ?? "";
        properties.Items["onboarding_goal"] = request.ObjetivoInicial ?? "";
        properties.Items["onboarding_source"] = request.OrigemConhecimento ?? "";
        properties.Items["onboarding_invite"] = request.CodigoConvite ?? "";
        properties.Items["login_only"] = request.ModoEntrar ? "true" : "false";
        return Challenge(properties, provider);
    }

    [HttpGet("externo/retorno")]
    public async Task<IActionResult> Return()
    {
        var info = await _signIn.GetExternalLoginInfoAsync();
        if (info == null || info.LoginProvider is not ("google" or "apple")
            || string.IsNullOrWhiteSpace(info.ProviderKey))
            return Redirect("/cadastro?externo=erro");

        try
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                ?? info.Principal.FindFirstValue("email");
            var loginOnly = info.AuthenticationProperties?.Items.TryGetValue("login_only", out var mode) == true && mode == "true";
            var resolution = await ExternalAccountResolver.ResolveAsync(
                _users, info.LoginProvider, info.ProviderKey, email, loginOnly);
            if (resolution.Status == ExternalAccountStatus.LoginNotLinked)
                return Redirect("/login?externo=conta-inexistente");
            if (resolution.Status == ExternalAccountStatus.EmailAlreadyRegistered)
                return Redirect("/cadastro?externo=conta-existente");
            var user = resolution.User;
            if (user == null)
            {
                var emailVerified = info.Principal.FindFirstValue("email_verified");
                if (string.IsNullOrWhiteSpace(email)
                    || !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
                    return Redirect("/cadastro?externo=erro");

                string Item(string key) => info.AuthenticationProperties?.Items.TryGetValue(key, out var value) == true ? value : null;
                var name = Item("onboarding_name")?.Trim();
                var goal = Item("onboarding_goal");
                var source = Item("onboarding_source");
                var inviteCode = Item("onboarding_invite")?.Trim();
                if (string.IsNullOrWhiteSpace(name) || name.Length > 100
                    || goal is not ("gastos" or "cartao" or "objetivos" or "saldo"))
                    return Redirect("/cadastro?externo=erro");
                if (!string.IsNullOrWhiteSpace(inviteCode)
                    && !(await _invites.CheckAsync(inviteCode, HttpContext.RequestAborted)).Valido)
                    return Redirect("/cadastro?externo=erro");

                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var created = await _users.CreateAsync(user);
                if (!created.Succeeded) return Redirect("/cadastro?externo=erro");
                var inviteConsumed = false;
                try
                {
                    var linked = await _users.AddLoginAsync(user, info);
                    if (!linked.Succeeded) throw new InvalidOperationException("Falha ao vincular identidade externa.");
                    var claims = new List<Claim> { new("name", name), new("onboarding_goal", goal) };
                    if (!string.IsNullOrWhiteSpace(source)) claims.Add(new Claim("referral_source", source));
                    if (!string.IsNullOrWhiteSpace(inviteCode)
                        && RegistrationInviteService.TryKey(inviteCode, out var adminId, out _))
                        claims.Add(new Claim("invited_by", adminId));
                    var added = await _users.AddClaimsAsync(user, claims);
                    if (!added.Succeeded) throw new InvalidOperationException("Falha ao salvar preferências de cadastro.");
                    if (!string.IsNullOrWhiteSpace(inviteCode))
                    {
                        inviteConsumed = await _invites.ConsumeAsync(inviteCode, user.Id, HttpContext.RequestAborted);
                        if (!inviteConsumed) throw new InvalidOperationException("Convite não está mais disponível.");
                    }
                    await _initialData.CriarDadosIniciaisAsync(user.Id, HttpContext.RequestAborted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Não foi possível concluir uma nova conta externa.");
                    if (inviteConsumed) await _invites.ReleaseAsync(inviteCode, user.Id, HttpContext.RequestAborted);
                    await _users.DeleteAsync(user);
                    return Redirect("/cadastro?externo=erro");
                }
            }

            if (await _users.IsLockedOutAsync(user)) return Redirect("/cadastro?externo=erro");
            var nonce = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            var nonceHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(nonce)));
            _db.UserTokens.Add(new IdentityUserToken<string>
            {
                UserId = user.Id,
                LoginProvider = "YourMoneyExternalTicket",
                Name = nonceHash,
                Value = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
            await _db.SaveChangesAsync(HttpContext.RequestAborted);
            var ticket = _ticketProtector.Protect(user.Id + ":" + nonce, TimeSpan.FromMinutes(5));
            Response.Cookies.Append(TicketCookie, ticket, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/identidade/externo/concluir",
                MaxAge = TimeSpan.FromMinutes(5)
            });
            return Redirect(loginOnly ? "/login?externo=concluido" : "/cadastro?externo=concluido");
        }
        finally
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    [HttpPost("externo/concluir")]
    public async Task<ActionResult> Complete()
    {
        var ticket = Request.Cookies[TicketCookie];
        Response.Cookies.Delete(TicketCookie, new CookieOptions { Path = "/api/identidade/externo/concluir" });
        if (string.IsNullOrWhiteSpace(ticket)) return Unauthorized();
        string ticketData;
        try { ticketData = _ticketProtector.Unprotect(ticket); }
        catch (Exception) { return Unauthorized(); }
        var separator = ticketData.LastIndexOf(':');
        if (separator < 1) return Unauthorized();
        var userId = ticketData[..separator];
        var nonce = ticketData[(separator + 1)..];
        var nonceHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(nonce)));
        var value = await _db.UserTokens.AsNoTracking()
            .Where(token => token.UserId == userId && token.LoginProvider == "YourMoneyExternalTicket"
                && token.Name == nonceHash)
            .Select(token => token.Value).SingleOrDefaultAsync(HttpContext.RequestAborted);
        if (!long.TryParse(value, out var expires)
            || expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return Unauthorized();
        var consumed = await _db.UserTokens
            .Where(token => token.UserId == userId && token.LoginProvider == "YourMoneyExternalTicket"
                && token.Name == nonceHash && token.Value == value)
            .ExecuteDeleteAsync(HttpContext.RequestAborted);
        if (consumed != 1) return Unauthorized();
        var user = await _users.FindByIdAsync(userId);
        if (user == null || await _users.IsLockedOutAsync(user)) return Unauthorized();
        return CustomResponse(await _jwtTokens.GenerateAsync(user));
    }
}
