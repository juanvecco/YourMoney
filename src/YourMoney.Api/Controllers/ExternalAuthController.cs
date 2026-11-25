using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
    private const string GoogleLinkStartCookie = "ym_google_link_start";
    private const string GoogleLinkFlowCookie = "ym_google_link_flow";
    private const string GoogleLinkPendingCookie = "ym_google_link_pending";
    private const string GoogleLinkStartPath = "/api/identidade/externo/vincular/google/iniciar";
    private const string GoogleLinkReturnPath = "/api/identidade/externo/retorno";
    private const string GoogleLinkPendingPath = "/api/identidade/vincular/google";
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly UserManager<IdentityUser> _users;
    private readonly IConfiguration _configuration;
    private readonly ITimeLimitedDataProtector _ticketProtector;
    private readonly JwtTokenService _jwtTokens;
    private readonly IUsuarioDemonstracaoService _initialData;
    private readonly ILogger<ExternalAuthController> _logger;
    private readonly RegistrationInviteService _invites;
    private readonly ApplicationDbContext _db;
    private readonly ITimeLimitedDataProtector _googleLinkStartProtector;
    private readonly ITimeLimitedDataProtector _googleLinkPendingProtector;
    private readonly ExternalAccountLinkService _accountLinks;

    public ExternalAuthController(
        SignInManager<IdentityUser> signIn,
        UserManager<IdentityUser> users,
        IConfiguration configuration,
        IDataProtectionProvider dataProtection,
        JwtTokenService jwtTokens,
        IUsuarioDemonstracaoService initialData,
        ILogger<ExternalAuthController> logger,
        RegistrationInviteService invites,
        ApplicationDbContext db,
        ExternalAccountLinkService accountLinks)
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
        _accountLinks = accountLinks;
        _googleLinkStartProtector = dataProtection.CreateProtector("YourMoney.GoogleLink.Start.v1").ToTimeLimitedDataProtector();
        _googleLinkPendingProtector = dataProtection.CreateProtector("YourMoney.GoogleLink.Pending.v1").ToTimeLimitedDataProtector();
    }

    [HttpGet("metodos")]
    public ActionResult Methods() => Ok(new
    {
        google = ExternalProviderConfiguration.GoogleAvailable(_configuration),
        apple = ExternalProviderConfiguration.AppleAvailable(_configuration)
    });

    [Authorize]
    [HttpGet("vinculos")]
    public async Task<ActionResult> GetAccountLinkStatus()
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var status = await _accountLinks.GetStatusAsync(userId, HttpContext.RequestAborted);
        return status == null ? Unauthorized() : CustomResponse(status);
    }

    [Authorize]
    [EnableCors("GoogleAccountLink")]
    [HttpPost("vincular/google/preparar")]
    public async Task<ActionResult> PrepareGoogleLink([FromBody] PrepareGoogleLinkRequest request)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!ExternalProviderConfiguration.GoogleAvailable(_configuration))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { mensagem = "A conexão com Google não está disponível no momento." });

        var result = await _accountLinks.PrepareAsync(userId, request.SenhaAtual, HttpContext.RequestAborted);
        if (!result.Succeeded) return MapLinkFailure(result.Failure);

        Response.Cookies.Delete(GoogleLinkPendingCookie, new CookieOptions { Path = GoogleLinkPendingPath, Secure = true, SameSite = SameSiteMode.None });

        var ticket = new GoogleLinkStartTicket(userId, result.Ticket!);
        Response.Cookies.Append(GoogleLinkStartCookie,
            _googleLinkStartProtector.Protect(JsonSerializer.Serialize(ticket), TimeSpan.FromMinutes(5)),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = GoogleLinkStartPath,
                MaxAge = TimeSpan.FromMinutes(5)
            });

        return CustomResponse(new PrepareGoogleLinkResponse(GoogleLinkStartPath));
    }

    [AllowAnonymous]
    [HttpPost("externo/vincular/google/iniciar")]
    public async Task<IActionResult> StartGoogleLink()
    {
        if (!IsAllowedFrontendOrigin()) return Forbid();
        var protectedTicket = Request.Cookies[GoogleLinkStartCookie];
        Response.Cookies.Delete(GoogleLinkStartCookie, new CookieOptions { Path = GoogleLinkStartPath, Secure = true, SameSite = SameSiteMode.None });
        if (string.IsNullOrWhiteSpace(protectedTicket)) return BadRequest(new { mensagem = "A tentativa expirou. Inicie novamente." });

        GoogleLinkStartTicket? ticket;
        try
        {
            var json = _googleLinkStartProtector.Unprotect(protectedTicket);
            ticket = JsonSerializer.Deserialize<GoogleLinkStartTicket>(json);
        }
        catch (Exception)
        {
            return BadRequest(new { mensagem = "A tentativa expirou. Inicie novamente." });
        }

        if (ticket == null || string.IsNullOrWhiteSpace(ticket.UserId) || string.IsNullOrWhiteSpace(ticket.Nonce)
            || !await _accountLinks.ConsumeStartTicketAsync(ticket.UserId, ticket.Nonce, HttpContext.RequestAborted))
            return BadRequest(new { mensagem = "A tentativa expirou. Inicie novamente." });
        if (!ExternalProviderConfiguration.GoogleAvailable(_configuration))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { mensagem = "A conexão com Google não está disponível no momento." });

        Response.Cookies.Append(GoogleLinkFlowCookie,
            _googleLinkStartProtector.Protect(JsonSerializer.Serialize(ticket), TimeSpan.FromMinutes(5)),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = GoogleLinkReturnPath,
                MaxAge = TimeSpan.FromMinutes(5)
            });

        var properties = _signIn.ConfigureExternalAuthenticationProperties("google", Url.Action(nameof(Return))!);
        properties.Items["account_link"] = "true";
        properties.Items["account_link_user"] = ticket.UserId;
        return Challenge(properties, "google");
    }

    [Authorize]
    [EnableCors("GoogleAccountLink")]
    [HttpGet("vincular/google/pendente")]
    public async Task<ActionResult> GetPendingGoogleLink()
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var pending = ReadPendingTicket();
        if (pending == null) return PendingGoogleLinkExpired();
        var result = await _accountLinks.GetPendingAsync(userId, pending, HttpContext.RequestAborted);
        return result == null ? PendingGoogleLinkExpired() : CustomResponse(result);
    }

    [Authorize]
    [EnableCors("GoogleAccountLink")]
    [HttpPost("vincular/google/confirmar")]
    public async Task<ActionResult> ConfirmGoogleLink()
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var pending = ReadPendingTicket();
        Response.Cookies.Delete(GoogleLinkPendingCookie, new CookieOptions { Path = GoogleLinkPendingPath, Secure = true, SameSite = SameSiteMode.None });
        if (pending == null) return PendingGoogleLinkExpired();

        var result = await _accountLinks.ConfirmAsync(userId, pending, HttpContext.RequestAborted);
        if (!result.Succeeded) return MapLinkFailure(result.Failure);
        return CustomResponse(result.Status);
    }

    [Authorize]
    [EnableCors("GoogleAccountLink")]
    [HttpPost("vincular/google/cancelar")]
    public async Task<ActionResult> CancelGoogleLink()
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var pending = ReadPendingTicket();
        Response.Cookies.Delete(GoogleLinkPendingCookie, new CookieOptions { Path = GoogleLinkPendingPath, Secure = true, SameSite = SameSiteMode.None });
        await _accountLinks.CancelPendingAsync(userId, pending, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost("externo/iniciar/{provider}")]
    public async Task<IActionResult> Start(string provider, [FromForm] CadastroExternoInicio request)
    {
        provider = provider?.ToLowerInvariant();
        if (provider is not ("google" or "apple")) return NotFound();
        if ((provider == "google" && !ExternalProviderConfiguration.GoogleAvailable(_configuration))
            || (provider == "apple" && !ExternalProviderConfiguration.AppleAvailable(_configuration)))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensagem = "Este método de acesso ainda não está disponível." });
        if (!ModelState.IsValid) return BadRequest(new { mensagem = "Revise seu nome e suas respostas antes de continuar." });
        var goals = OnboardingGoals.Resolve(request.ObjetivosIniciais, request.ObjetivoInicial);
        if (!request.ModoEntrar && (string.IsNullOrWhiteSpace(request.Nome)
            || goals == null))
            return BadRequest(new { mensagem = "Revise seu nome e suas respostas antes de continuar." });
        if (!string.IsNullOrWhiteSpace(request.CodigoConvite)
            && !(await _invites.CheckAsync(request.CodigoConvite.Trim(), HttpContext.RequestAborted)).Valido)
            return Redirect("/cadastro?externo=erro");

        var properties = _signIn.ConfigureExternalAuthenticationProperties(provider, Url.Action(nameof(Return))!);
        properties.Items["onboarding_name"] = request.Nome?.Trim() ?? "";
        properties.Items["onboarding_goal"] = goals?.FirstOrDefault() ?? "";
        properties.Items["onboarding_goals"] = string.Join(',', goals ?? []);
        properties.Items["onboarding_source"] = request.OrigemConhecimento ?? "";
        properties.Items["onboarding_invite"] = request.CodigoConvite ?? "";
        properties.Items["login_only"] = request.ModoEntrar ? "true" : "false";
        return Challenge(properties, provider);
    }

    [HttpGet("externo/retorno")]
    public async Task<IActionResult> Return()
    {
        var linkFlowCookiePresent = Request.Cookies.ContainsKey(GoogleLinkFlowCookie);
        GoogleLinkStartTicket? linkFlowTicket = null;
        if (linkFlowCookiePresent)
        {
            var protectedFlow = Request.Cookies[GoogleLinkFlowCookie];
            Response.Cookies.Delete(GoogleLinkFlowCookie, new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = GoogleLinkReturnPath
            });
            try
            {
                linkFlowTicket = JsonSerializer.Deserialize<GoogleLinkStartTicket>(_googleLinkStartProtector.Unprotect(protectedFlow!));
            }
            catch (Exception)
            {
                return Redirect("/configuracao?secao=seguranca&google=erro");
            }
        }

        var info = await _signIn.GetExternalLoginInfoAsync();
        if (linkFlowCookiePresent)
        {
            if (info == null || info.LoginProvider != "google" || string.IsNullOrWhiteSpace(info.ProviderKey)
                || string.IsNullOrWhiteSpace(linkFlowTicket?.UserId))
                return Redirect("/configuracao?secao=seguranca&google=erro");
            return await ReturnFromGoogleLinkAsync(info, linkFlowTicket.UserId);
        }

        if (info == null || info.LoginProvider is not ("google" or "apple")
            || string.IsNullOrWhiteSpace(info.ProviderKey))
            return Redirect("/cadastro?externo=erro");

        try
        {
            if (info.AuthenticationProperties?.Items.TryGetValue("account_link", out var linkMode) == true
                && linkMode == "true")
                return await ReturnFromGoogleLinkAsync(info);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                ?? info.Principal.FindFirstValue("email");
            var loginOnly = info.AuthenticationProperties?.Items.TryGetValue("login_only", out var mode) == true && mode == "true";
            var resolution = await ExternalAccountResolver.ResolveAsync(
                _users, info.LoginProvider, info.ProviderKey, email, loginOnly);
            if (resolution.Status == ExternalAccountStatus.LoginNotLinked)
                return Redirect(info.LoginProvider == "google"
                    ? "/login?externo=google-nao-vinculado"
                    : "/login?externo=conta-inexistente");
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
                var goals = OnboardingGoals.Resolve(
                    Item("onboarding_goals")?.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    Item("onboarding_goal"));
                var source = Item("onboarding_source");
                var inviteCode = Item("onboarding_invite")?.Trim();
                if (string.IsNullOrWhiteSpace(name) || name.Length > 100
                    || goals == null)
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
                    var claims = new List<Claim> { new("name", name) };
                    claims.AddRange(goals.Select(goal => new Claim("onboarding_goal", goal)));
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

    private async Task<IActionResult> ReturnFromGoogleLinkAsync(ExternalLoginInfo info, string? intendedUserId = null)
    {
        var properties = info.AuthenticationProperties;
        var userId = !string.IsNullOrWhiteSpace(intendedUserId)
            ? intendedUserId
            : properties?.Items.TryGetValue("account_link_user", out var storedUserId) == true
            ? storedUserId
            : null;
        if (info.LoginProvider != "google" || string.IsNullOrWhiteSpace(userId))
            return Redirect("/configuracao?secao=seguranca&google=erro");

        var user = await _users.FindByIdAsync(userId);
        if (user == null || await _users.IsLockedOutAsync(user))
            return Redirect("/configuracao?secao=seguranca&google=erro");

        var email = info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("email");
        var emailVerified = info.Principal.FindFirstValue("email_verified");
        if (!ExternalAccountLinkService.IsVerifiedEmailMatch(user.Email, email, emailVerified, _users.KeyNormalizer!))
            return Redirect("/configuracao?secao=seguranca&google=email-diferente");

        var pending = await _accountLinks.CreatePendingAsync(user.Id, info.ProviderKey, email!, HttpContext.RequestAborted);
        if (!pending.Succeeded)
        {
            return pending.Failure switch
            {
                ExternalAccountLinkFailure.AlreadyLinked => Redirect("/configuracao?secao=seguranca&google=conectado"),
                ExternalAccountLinkFailure.IdentityConflict => Redirect("/configuracao?secao=seguranca&google=conflito"),
                _ => Redirect("/configuracao?secao=seguranca&google=erro")
            };
        }

        var ticket = new GoogleLinkPendingTicket(user.Id, info.ProviderKey, email!, pending.Ticket!);
        Response.Cookies.Append(GoogleLinkPendingCookie,
            _googleLinkPendingProtector.Protect(JsonSerializer.Serialize(ticket), TimeSpan.FromMinutes(5)),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = GoogleLinkPendingPath,
                MaxAge = TimeSpan.FromMinutes(5)
            });
        return Redirect("/configuracao?secao=seguranca&google=pendente");
    }

    private string? CurrentUserId() =>
        User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

    private GoogleLinkPendingTicket? ReadPendingTicket()
    {
        var protectedTicket = Request.Cookies[GoogleLinkPendingCookie];
        if (string.IsNullOrWhiteSpace(protectedTicket)) return null;
        try
        {
            return JsonSerializer.Deserialize<GoogleLinkPendingTicket>(_googleLinkPendingProtector.Unprotect(protectedTicket));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private bool IsAllowedFrontendOrigin()
    {
        var origin = Request.Headers.Origin.FirstOrDefault();
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var requestedOrigin)) return false;
        var allowed = new List<string>();
        var frontend = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(frontend)) allowed.Add(frontend);
        allowed.AddRange(_configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? []);
        allowed.Add($"{Request.Scheme}://{Request.Host}");
        return allowed.Any(candidate => Uri.TryCreate(candidate, UriKind.Absolute, out var allowedOrigin)
            && string.Equals(allowedOrigin.GetLeftPart(UriPartial.Authority), requestedOrigin.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase));
    }

    private ActionResult MapLinkFailure(ExternalAccountLinkFailure failure) => failure switch
    {
        ExternalAccountLinkFailure.InvalidPassword => BadRequest(new { mensagem = "A senha não confere. Confira a senha da sua conta YourMoney e tente novamente." }),
        ExternalAccountLinkFailure.AccountLocked => Unauthorized(new { mensagem = "Por segurança, entre novamente na sua conta antes de conectar o Google." }),
        ExternalAccountLinkFailure.AccountNotFound => Unauthorized(),
        ExternalAccountLinkFailure.AlreadyLinked => Conflict(new { mensagem = "O Google já está conectado a esta conta." }),
        ExternalAccountLinkFailure.EmailMismatch => UnprocessableEntity(new { mensagem = "O Google precisa usar o mesmo e-mail verificado da sua conta YourMoney." }),
        ExternalAccountLinkFailure.IdentityConflict => Conflict(new { mensagem = "Esta conta Google já está conectada a outra conta YourMoney." }),
        ExternalAccountLinkFailure.InvalidTicket => PendingGoogleLinkExpired(),
        _ => Unauthorized(new { mensagem = "A tentativa expirou ou já foi usada. Inicie novamente." })
    };

    private ActionResult PendingGoogleLinkExpired() =>
        StatusCode(StatusCodes.Status410Gone, new { mensagem = "A confirmação expirou ou não está mais disponível. Inicie novamente a conexão com Google." });
}
