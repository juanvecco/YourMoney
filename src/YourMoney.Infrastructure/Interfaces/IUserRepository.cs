using YourMoney.Domain.Entities;

namespace YourMoney.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
    }
}