using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Data;

namespace YourMoney.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AuthService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        // ... Implementação do LoginAsync e RefreshTokenAsync ...

        // --- LoginAsync ---
        public async Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Username);

            if (user == null) return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded) return null;

            var authResult = await GenerateAuthResult(user);

            // Salvar Refresh Token
            var refreshToken = GenerateRefreshToken(ipAddress, user.Id);
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            authResult.RefreshToken = refreshToken.Token;
            return authResult;
        }

        // --- RefreshTokenAsync ---
        public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var existingToken = await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(t => t.Token == refreshToken);

            if (existingToken == null) return null; // Token não encontrado

            // 1. Revogação em Cascata (Detecção de Reuso de Token)
            if (existingToken.IsActive == false)
            {
                // Se o token não está ativo (já foi revogado ou expirou), mas está sendo usado,
                // isso indica um possível reuso malicioso. Revogamos todos os tokens do usuário.
                await RevokeAllUserTokens(existingToken.UserId);
                return null;
            }

            // 2. Validação de IP (Segurança Adicional)
            if (existingToken.CreatedByIp != ipAddress)
            {
                // Opcional: Revogar o token se o IP for diferente.
                existingToken.Revoked = DateTime.UtcNow;
                existingToken.RevokedByIp = ipAddress;
                await _dbContext.SaveChangesAsync();
                return null;
            }

            // Revogar o token antigo e gerar um novo
            existingToken.Revoked = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;
            existingToken.ReplacedByToken = GenerateUniqueToken(); // Token de substituição

            var user = await _userManager.FindByIdAsync(existingToken.UserId);
            if (user == null) return null;

            var newAuthResult = await GenerateAuthResult(user);

            // Gerar novo Refresh Token
            var newRefreshToken = GenerateRefreshToken(ipAddress, user.Id);
            // O novo token não precisa herdar o ReplacedByToken do antigo.
            // A cadeia de substituição é implícita pelo uso do token.

            _dbContext.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync();

            newAuthResult.RefreshToken = newRefreshToken.Token;
            return newAuthResult;
        }

        // --- Métodos Auxiliares ---

        private async Task<AuthResult> GenerateAuthResult(IdentityUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var jwtToken = GenerateJwtToken(user, claims, roles);

            return new AuthResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Expiration = jwtToken.ValidTo,
                Username = user.Email
            };
        }

        private JwtSecurityToken GenerateJwtToken(IdentityUser user, IList<Claim> claims, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];
            var expirationInMinutes = int.Parse(jwtSettings["ExpirationInMinutes"]);

            var allClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            // Adicionar claims e roles existentes
            allClaims.AddRange(claims);
            allClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(expirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: allClaims,
                expires: expires,
                signingCredentials: creds
            );

            return token;
        }

        private RefreshToken GenerateRefreshToken(string ipAddress, string userId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"]);

            return new RefreshToken
            {
                Token = GenerateUniqueToken(),
                Expires = DateTime.UtcNow.AddDays(expirationDays),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = userId
            };
        }

        private string GenerateUniqueToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task RevokeAllUserTokens(string userId)
        {
            // Revoga todos os tokens ativos e futuros do usuário
            var tokens = await _dbContext.RefreshTokens
                .Where(t => t.UserId == userId && t.Revoked == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoked = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}