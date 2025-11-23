using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private string IpAddress => Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString();

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResult), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request, IpAddress);

            if (result == null)
            {
                return Unauthorized(new { Message = "Credenciais inválidas." });
            }

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResult), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken, IpAddress);

            if (result == null)
            {
                return Unauthorized(new { Message = "Refresh Token inválido ou expirado." });
            }

            return Ok(result);
        }

        [HttpPost("register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true // Simplificando para o exemplo
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // Opcional: Adicionar uma Role padrão aqui, ex: await userManager.AddToRoleAsync(user, "User");
                return Ok(new { Message = "Usuário registrado com sucesso. Prossiga para o login." });
            }

            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }
    }
}