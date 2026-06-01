using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var id = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user?.FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(id))
                    throw new InvalidOperationException("Usuario autenticado nao identificado.");

                return id;
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }
}
