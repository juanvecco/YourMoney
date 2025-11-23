using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Interfaces;

namespace YourMoney.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        // Em um projeto real, esta seria a lógica de acesso ao banco de dados.
        // Aqui, simulamos um usuário para fins de demonstração.
        private readonly List<User> _users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                Username = "teste@email.com",
                // A senha "senha123" hasheada. Use uma biblioteca como BCrypt para hashing.
                // Para este exemplo, vamos simplificar a validação na camada Application.
                PasswordHash = "senha123",
                Role = "User"
            }
        };

        public Task<User> GetUserByUsernameAsync(string username)
        {
            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }
    }
}