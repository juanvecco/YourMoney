using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class InvestimentoRepositoryReferenceTests
    {
        public static Task RepositoryUsesReferenceWithLegacyFallback()
        {
            var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "InvestimentoRepository.cs"));

            TestAssert.True(source.Contains("r.MesReferencia.HasValue"), "Repository should prefer reference");
            TestAssert.True(source.Contains("!r.MesReferencia.HasValue"), "Repository should include legacy fallback");
            TestAssert.True(source.Contains("r.UsuarioId == usuarioId"), "Query should remain owner scoped");
            return Task.CompletedTask;
        }

        public static Task ConfigurationAndMigrationAreAdditive()
        {
            var configuration = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Configurations", "InvestimentoConfiguration.cs"));
            var migration = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Migrations", "20260613000000_AddInvestimentoMesReferencia.cs"));
            var snapshot = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Migrations", "AppDbContextModelSnapshot.cs"));

            TestAssert.True(configuration.Contains("HasColumnName(\"mesReferencia\")"), "Configuration should map column");
            TestAssert.True(configuration.Contains("HasColumnType(\"date\")"), "Column should be SQL date");
            TestAssert.True(configuration.Contains("IsRequired(false)"), "Legacy references should allow null");
            TestAssert.True(migration.Contains("nullable: true"), "Migration should be nullable");
            TestAssert.True(!migration.Contains("UpdateData"), "Migration should not backfill data");
            TestAssert.True(snapshot.Contains("IX_Investimento_MesReferencia"), "Snapshot should include reference index");
            return Task.CompletedTask;
        }
    }
}
