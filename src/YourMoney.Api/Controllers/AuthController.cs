using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Extensions;
using YourMoney.Api.Models;
using System.Security.Claims;
using YourMoney.Api.Services;

namespace YourMoney.Api.Controllers
{
    [Route("api/identidade")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtTokenService _jwtTokens;
        private readonly IPasswordResetDeliveryService _passwordResetDelivery;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IUsuarioDemonstracaoService _usuarioDemonstracaoService;
        private readonly RegistrationInviteService _invites;

        public AuthController(SignInManager<IdentityUser> signInManager,
                              UserManager<IdentityUser> userManager,
                              JwtTokenService jwtTokens,
                              IPasswordResetDeliveryService passwordResetDelivery,
                              IConfiguration configuration,
                              ILogger<AuthController> logger,
                              IUsuarioDemonstracaoService usuarioDemonstracaoService,
                              RegistrationInviteService invites)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtTokens = jwtTokens;
            _passwordResetDelivery = passwordResetDelivery;
            _configuration = configuration;
            _logger = logger;
            _usuarioDemonstracaoService = usuarioDemonstracaoService;
            _invites = invites;
        }

        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var inviteCode = usuarioRegistro.CodigoConvite?.Trim();
            if (!string.IsNullOrWhiteSpace(inviteCode))
            {
                var check = await _invites.CheckAsync(inviteCode, HttpContext.RequestAborted);
                if (!check.Valido)
                {
                    AdicionarErroProcessamento(check.Motivo);
                    return CustomResponse();
                }
            }

            var user = new IdentityUser
            {
                UserName = usuarioRegistro.Email,
                Email = usuarioRegistro.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, usuarioRegistro.Senha);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    AdicionarErroProcessamento(error.Description);
                }

                return CustomResponse();
            }

            var claims = new List<Claim>
            {
                new("name", usuarioRegistro.Nome.Trim()),
                new("onboarding_goal", usuarioRegistro.ObjetivoInicial ?? "gastos")
            };
            if (!string.IsNullOrWhiteSpace(usuarioRegistro.OrigemConhecimento))
                claims.Add(new Claim("referral_source", usuarioRegistro.OrigemConhecimento.Trim()));
            if (!string.IsNullOrWhiteSpace(inviteCode)
                && RegistrationInviteService.TryKey(inviteCode, out var adminId, out _))
                claims.Add(new Claim("invited_by", adminId));

            var claimResult = await _userManager.AddClaimsAsync(user, claims);
            if (!claimResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                AdicionarErroProcessamento("Não foi possível preparar sua conta. Tente novamente.");
                return CustomResponse();
            }

            if (!string.IsNullOrWhiteSpace(inviteCode)
                && !await _invites.ConsumeAsync(inviteCode, user.Id, HttpContext.RequestAborted))
            {
                await _userManager.DeleteAsync(user);
                AdicionarErroProcessamento("Este convite não está mais disponível. Remova-o ou peça outro link.");
                return CustomResponse();
            }

            try
            {
                await _usuarioDemonstracaoService.CriarDadosIniciaisAsync(user.Id, HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Não foi possível preparar os dados iniciais da nova conta.");
                if (!string.IsNullOrWhiteSpace(inviteCode))
                    await _invites.ReleaseAsync(inviteCode, user.Id, HttpContext.RequestAborted);
                await _userManager.DeleteAsync(user);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensagem = "Não foi possível preparar sua conta. Tente novamente."
                });
            }

            var token = await GerarJwt(user.Email);
            return CustomResponse(token);
        }

        [HttpPost("autenticar")]
        public async Task<ActionResult> Login(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, false, true);

            if (result.Succeeded) return CustomResponse(await GerarJwt(usuarioLogin.Email));

            if(result.IsLockedOut)
            {
                AdicionarErroProcessamento("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse();
            }

            AdicionarErroProcessamento("Usuário ou senha incorretos");

            return CustomResponse();
        }

        [HttpPost("esqueci-minha-senha")]
        public async Task<ActionResult> EsqueciMinhaSenha(UsuarioEsqueceuSenha usuarioEsqueceuSenha)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await _userManager.FindByEmailAsync(usuarioEsqueceuSenha.Email);
            if (user != null)
            {
                try
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/')
                        ?? $"{Request.Scheme}://{Request.Host}";
                    var resetUrl = frontendBaseUrl
                        + "/redefinir-senha?email=" + Uri.EscapeDataString(user.Email ?? usuarioEsqueceuSenha.Email)
                        + "&token=" + Uri.EscapeDataString(token);
                    await _passwordResetDelivery.DeliverAsync(user.Email!, resetUrl, HttpContext.RequestAborted);
                }
                catch (Exception)
                {
                    _logger.LogWarning("A solicitação de recuperação foi aceita, mas a entrega não pôde ser concluída.");
                }
            }

            return CustomResponse(new
            {
                mensagem = "Se o e-mail estiver cadastrado, enviaremos as instruções para redefinir a senha."
            });
        }

        [HttpPost("resetar-senha")]
        public async Task<ActionResult> ResetarSenha(UsuarioResetSenha usuarioResetSenha)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await _userManager.FindByEmailAsync(usuarioResetSenha.Email);

            if (user == null)
            {
                AdicionarErroProcessamento("Não foi possível redefinir a senha. Solicite um novo link.");
                return CustomResponse();
            }

            var result = await _userManager.ResetPasswordAsync(user, usuarioResetSenha.Token, usuarioResetSenha.Senha);

            if (!result.Succeeded)
            {
                AdicionarErroProcessamento("Não foi possível redefinir a senha. Solicite um novo link ou revise os requisitos da nova senha.");
                return CustomResponse();
            }

            return CustomResponse(new
            {
                mensagem = "Senha redefinida com sucesso."
            });
        }

        private async Task<UsuarioRespostaLogin> GerarJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("Usuário não encontrado ao gerar JWT.");
            return await _jwtTokens.GenerateAsync(user);
        }

    }
}
