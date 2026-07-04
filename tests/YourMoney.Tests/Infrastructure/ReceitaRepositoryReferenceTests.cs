using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class ReceitaRepositoryReferenceTests
    {
        public static Task RepositoryUsesReferenceWithLegacyFallback()
        {
            var repositoryPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "ReceitaRepository.cs");
            var source = File.ReadAllText(repositoryPath);

            TestAssert.True(source.Contains("r.MesReferencia.HasValue"), "Repository should prefer MesReferencia");
            TestAssert.True(source.Contains("!r.MesReferencia.HasValue && r.Data.Month"), "Repository should fall back to Data for legacy rows");
            TestAssert.True(source.Contains("r.UsuarioId == usuarioId"), "Reference queries should remain owner scoped");
            return Task.CompletedTask;
        }

        public static Task ConfigurationMapsExistingNullableDateColumn()
        {
            var configurationPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Configurations", "ReceitaConfiguration.cs");
            var source = File.ReadAllText(configurationPath);

            TestAssert.True(source.Contains("HasColumnName(\"mesReferencia\")"), "Configuration should map the existing column");
            TestAssert.True(source.Contains("HasColumnType(\"date\")"), "Reference column should use SQL date");
            TestAssert.True(source.Contains("IsRequired(false)"), "Existing legacy rows should allow null reference");
            return Task.CompletedTask;
        }

        public static Task RepositorySupportsEligibleRevenueAndReimbursements()
        {
            var repositoryPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "ReceitaRepository.cs");
            var repositorySource = File.ReadAllText(repositoryPath);
            var configurationPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Configurations", "ReceitaConfiguration.cs");
            var configurationSource = File.ReadAllText(configurationPath);
            var migrationPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Migrations", "20260704000000_AddReceitaNatureza.cs");
            var migrationSource = File.ReadAllText(migrationPath);

            TestAssert.True(repositorySource.Contains("GetTotalElegivelMetasByMesAnoAsync"), "Repository should expose eligible revenue query");
            TestAssert.True(repositorySource.Contains("NaturezaReceita.RendaDisponivel"), "Eligible revenue query should filter available income");
            TestAssert.True(repositorySource.Contains("GetTotalReembolsadoPorDespesaAsync"), "Repository should expose reimbursement lookup");
            TestAssert.True(repositorySource.Contains("DespesaVinculadaId"), "Reimbursement query should use linked expense");
            TestAssert.True(configurationSource.Contains("HasDefaultValue(NaturezaReceita.RendaDisponivel)"), "Configuration should default legacy rows to available income");
            TestAssert.True(configurationSource.Contains("OnDelete(DeleteBehavior.SetNull)"), "Linked expense delete should not delete receitas");
            TestAssert.True(migrationSource.Contains("Natureza"), "Migration should add nature column");
            TestAssert.True(migrationSource.Contains("DespesaVinculadaId"), "Migration should add linked expense column");
            return Task.CompletedTask;
        }
    }
}
