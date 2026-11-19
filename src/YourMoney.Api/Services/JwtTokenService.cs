using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YourMoney.Api.Models;
using YourMoney.Api.Extensions;

namespace YourMoney.Api.Services;

public sealed class JwtTokenService
{
    private readonly UserManager<IdentityUser> _users;
    private readonly JwtSettings _settings;

    public JwtTokenService(UserManager<IdentityUser> users, IOptions<JwtSettings> settings)
    {
        _users = users;
        _settings = settings.Value;
    }

    public async Task<UsuarioRespostaLogin> GenerateAsync(IdentityUser user)
    {
        var storedClaims = (await _users.GetClaimsAsync(user)).ToList();
        var roles = (await _users.GetRolesAsync(user)).ToList();
        var claims = new List<Claim>(storedClaims)
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (!claims.Any(claim => claim.Type == "name"))
            claims.Add(new Claim("name", user.UserName ?? ""));
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_settings.Secret);
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _settings.Emissor,
            Audience = _settings.ValidoEm,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });
        return new UsuarioRespostaLogin
        {
            AccessToken = tokenHandler.WriteToken(token),
            ExpiresIn = TimeSpan.FromMinutes(_settings.ExpirationInMinutes).TotalSeconds,
            UsuarioToken = new UsuarioToken
            {
                Id = user.Id,
                Nome = storedClaims.FirstOrDefault(claim => claim.Type == "name")?.Value ?? user.UserName,
                Email = user.Email,
                Roles = roles,
                Claims = storedClaims.Select(claim => new UsuarioClaim { Type = claim.Type, Value = claim.Value })
            }
        };
    }
}
