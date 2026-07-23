using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class ReceitaRecorrenteRepositoryTests
    {
        public static Task MappingCreatesPreciseTablesAndUniqueOccurrences()
        {
            var migrationDirectory = RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Migrations");
            var migrationPath = Directory.GetFiles(migrationDirectory, "*AddReceitasRecorrentesReserva.cs").Single();
            var migration = File.ReadAllText(migrationPath);
            var occurrenceConfiguration = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Configurations", "ReceitaRecorrenteOcorrenciaConfiguration.cs"));

            TestAssert.True(migration.Contains("tbReceitaRecorrente"), "Migration should create recurrence table");
            TestAssert.True(migration.Contains("tbReceitaRecorrenteOcorrencia"), "Migration should create occurrence table");
            TestAssert.True(migration.Contains("decimal(18,2)"), "Migration should preserve monetary precision");
            TestAssert.True(migration.Contains("UX_ReceitaRecorrenteOcorrencia_Usuario_Recorrencia_Mes"), "Migration should prevent duplicate monthly occurrences");
            TestAssert.True(migration.Contains("IdContaFinanceira") && migration.Contains("nullable: true"), "Legacy Receita account should remain nullable");
            TestAssert.True(occurrenceConfiguration.Contains("HasConversion<string>()"), "Occurrence status should persist as readable string");
            TestAssert.True(occurrenceConfiguration.Contains("IsUnique()"), "Occurrence mapping should enforce uniqueness");
            return Task.CompletedTask;
        }

        public static Task RepositoryKeepsEveryReadUserScoped()
        {
            var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "ReceitaRecorrenteRepository.cs"));

            TestAssert.True(source.Contains("r.UsuarioId == usuarioId"), "Recurrence queries should filter authenticated owner");
            TestAssert.True(source.Contains("o.UsuarioId == usuarioId"), "Occurrence queries should filter authenticated owner");
            TestAssert.True(source.Contains("r.ConsideraReservaEmergencia"), "Projection query should include only marked bases");
            TestAssert.True(source.Contains("r.Ativa"), "Monthly and projection reads should include only active recurrences");
            TestAssert.True(source.Contains("MesReferencia == mes"), "Occurrence reads should normalize selected month");
            return Task.CompletedTask;
        }
    }
}
