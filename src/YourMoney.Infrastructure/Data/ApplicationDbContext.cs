using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace YourMoney.Infrastructure.Data
{
    // Use IdentityUser se não precisar de campos extras, ou crie sua própria classe
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Configurações adicionais do modelo, se necessário
        }
    }
}