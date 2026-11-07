using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YourMoney.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING é obrigatória para operações de design-time.");

            var provider = Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? "SqlServer";
            if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
                || provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                optionsBuilder.UseNpgsql(connectionString);
            else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                optionsBuilder.UseSqlServer(connectionString);
            else
                throw new InvalidOperationException("DATABASE_PROVIDER inválido.");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
