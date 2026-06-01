using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using YourMoney.Api.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class CurrentUserServiceTests
    {
        public static Task ResolvesUserIdFromSupportedJwtClaims()
        {
            foreach (var claimType in new[] { JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier, "sub" })
            {
                var service = CreateService(new Claim(claimType, $"user-{claimType}"));

                TestAssert.True(service.UserId == $"user-{claimType}", $"CurrentUserService should read {claimType}");
            }

            return Task.CompletedTask;
        }

        public static Task ThrowsWhenAuthenticatedUserIdIsMissing()
        {
            var service = CreateService(new Claim(ClaimTypes.Email, "user@example.com"));

            try
            {
                _ = service.UserId;
            }
            catch (InvalidOperationException)
            {
                return Task.CompletedTask;
            }

            throw new InvalidOperationException("CurrentUserService should fail clearly when user id claim is missing");
        }

        private static CurrentUserService CreateService(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            };

            return new CurrentUserService(new HttpContextAccessor { HttpContext = context });
        }
    }
}
