using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Domain.Repositories;
using YourMoney.Infrastructure.Persistence;
using YourMoney.Infrastructure.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure;

public static class GrupoObjetivoInvestimentoTransacaoTests
{
    public static Task UsesSerializableSetBasedAtomicCommand()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Repositories", "GrupoObjetivoInvestimentoRepository.cs"));
        TestAssert.True(source.Contains("IsolationLevel.Serializable"), "Exclusion should use serializable transaction");
        TestAssert.True(source.Contains("ExecuteUpdateAsync"), "Link mutations should be set based");
        TestAssert.True(source.Contains("CommitAsync") && source.Contains("RollbackAsync"), "Exclusion should commit or roll back atomically");
        TestAssert.True(source.Contains("VersaoDestino") && source.Contains("VersaoOrigem"), "Both concurrency tokens should be revalidated");
        TestAssert.True(source.IndexOf("ExecuteUpdateAsync", StringComparison.Ordinal) < source.IndexOf("CommitAsync", StringComparison.Ordinal), "Link update must remain inside transaction");
        return Task.CompletedTask;
    }

    public static async Task RollsBackInjectedDeleteFailureWhenSqlServerIsConfigured()
    {
        var connectionString = Environment.GetEnvironmentVariable("YOURMONEY_TRANSACTION_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
        await using var context = new AppDbContext(options);
        var userId = $"feature-023-rollback-{Guid.NewGuid():N}";
        var source = new GrupoObjetivoInvestimento("Origem rollback", userId);
        var destination = new GrupoObjetivoInvestimento("Destino rollback", userId);
        var investment = new Investimento(
            "Investimento rollback", "Validação transacional", "Produto legado", 1m, 50m, 50m,
            new DateTime(2026, 8, 1), new DateTime(2026, 8, 1), userId,
            grupoObjetivoInvestimentoId: source.Id, tipoAtivo: TipoAtivo.RendaFixa);

        try
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO [AspNetUsers] ([Id],[EmailConfirmed],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnabled],[AccessFailedCount])
                VALUES ({userId},0,0,0,0,0);
                """);
            context.GruposObjetivoInvestimento.AddRange(source, destination);
            context.Investimentos.Add(investment);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            await context.Database.ExecuteSqlRawAsync("""
                CREATE OR ALTER TRIGGER [TR_Feature023_ForceGroupDeleteFailure]
                ON [tbInvestimentoGrupoObjetivo]
                INSTEAD OF DELETE
                AS THROW 51023, 'Falha injetada para validar rollback.', 1;
                """);

            var repository = new GrupoObjetivoInvestimentoRepository(context);
            var failed = false;
            try
            {
                await repository.ExcluirAsync(new ExcluirGrupoObjetivoCommand(
                    source.Id, userId, source.Versao, AcaoVinculosGrupoObjetivo.Transferir,
                    destination.Id, destination.Versao));
            }
            catch
            {
                failed = true;
            }

            TestAssert.True(failed, "Injected delete failure should reach the repository rollback path");
            context.ChangeTracker.Clear();
            var persistedInvestment = await context.Investimentos.AsNoTracking().SingleAsync(i => i.Id == investment.Id);
            TestAssert.Equal(source.Id, persistedInvestment.GrupoObjetivoInvestimentoId, "Rollback should restore the original investment link");
            TestAssert.True(await context.GruposObjetivoInvestimento.AnyAsync(g => g.Id == source.Id), "Rollback should preserve the source group");
            TestAssert.True(await context.GruposObjetivoInvestimento.AnyAsync(g => g.Id == destination.Id), "Rollback should preserve the destination group");
        }
        finally
        {
            context.ChangeTracker.Clear();
            await context.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS [TR_Feature023_ForceGroupDeleteFailure];");
            await context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM [tbInvestimento] WHERE [Id] = {investment.Id};");
            await context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM [tbInvestimentoGrupoObjetivo] WHERE [Id] IN ({source.Id}, {destination.Id});");
            await context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM [AspNetUsers] WHERE [Id] = {userId};");
        }
    }
}
