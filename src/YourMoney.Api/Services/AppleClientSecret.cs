using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using YourMoney.Api.Configuration;

namespace YourMoney.Api.Services;

public static class AppleClientSecret
{
    public static string Create(ExternalProviderSettings settings)
    {
        var pem = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(settings.PrivateKeyBase64));
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(pem);
        var key = new ECDsaSecurityKey(ecdsa) { KeyId = settings.KeyId };
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.TeamId,
            Audience = "https://appleid.apple.com",
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(5),
            Subject = new System.Security.Claims.ClaimsIdentity([
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, settings.ClientId)
            ]),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256)
        };
        return new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);
    }
}
