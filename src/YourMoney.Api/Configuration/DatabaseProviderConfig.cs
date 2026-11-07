using Microsoft.EntityFrameworkCore;

namespace YourMoney.Api.Configuration;

internal static class DatabaseProviderConfig
{
    public static void Configure(
        DbContextOptionsBuilder options,
        IConfiguration configuration,
        string connectionName)
    {
        var connectionString = configuration["DATABASE_CONNECTION_STRING"]
            ?? configuration.GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string {connectionName} não encontrada.");

        var provider = configuration["DATABASE_PROVIDER"]
            ?? configuration["Database:Provider"]
            ?? "SqlServer";

        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            options.UseNpgsql(connectionString);
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlServer(connectionString);
        }
        else
        {
            throw new InvalidOperationException($"Provedor de banco não suportado: {provider}.");
        }
    }
}
