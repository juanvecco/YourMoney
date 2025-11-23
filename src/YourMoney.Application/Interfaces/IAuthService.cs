using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress);
        Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress);
    }
}