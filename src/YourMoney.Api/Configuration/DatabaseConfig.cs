using Microsoft.EntityFrameworkCore;
using YourMoney.Infrastructure.Persistence;

namespace YourMoney.Api.Configuration
{
    public static class DatabaseConfig
    {
        public static IServiceCollection AddDatabaseConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string não encontrada");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
