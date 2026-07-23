using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class DespesaRecorrenteRepositoryTests
    {
        public static Task RecurringExpenseMappingCreatesTablesAndUniqueMonthlyOccurrence()
        {
            var migration = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src",
                "YourMoney.Infrastructure",
                "Migrations",
                "20260706000000_AddDespesasRecorrentes.cs"));
            var occurrenceConfiguration = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src",
                "YourMoney.Infrastructure",
                "Configurations",
                "DespesaRecorrenteOcorrenciaConfiguration.cs"));

            TestAssert.True(migration.Contains("tbDespesaRecorrente"), "Migration should create recurring expense table");
            TestAssert.True(migration.Contains("tbDespesaRecorrenteOcorrencia"), "Migration should create monthly occurrence table");
            TestAssert.True(migration.Contains("UX_DespesaRecorrenteOcorrencia_Usuario_Recorrencia_Mes"), "Migration should enforce one occurrence per recurrence and month");
            TestAssert.True(migration.Contains("IX_tbDespesaRecorrenteOcorrencia_DespesaRecorrenteId"), "Migration should include EF convention index for recurring occurrence relationship");
            TestAssert.True(occurrenceConfiguration.Contains("HasConversion<string>()"), "Occurrence status should be persisted as string");
            TestAssert.True(occurrenceConfiguration.Contains("IsUnique()"), "Occurrence configuration should keep monthly uniqueness");

            return Task.CompletedTask;
        }

        public static Task RecurringExpenseRepositoryKeepsUserScopedQueries()
        {
            var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src",
                "YourMoney.Infrastructure",
                "Repositories",
                "DespesaRecorrenteRepository.cs"));

            TestAssert.True(source.Contains("r.UsuarioId == usuarioId"), "Recurring repository should filter recurring configurations by user");
            TestAssert.True(source.Contains("o.UsuarioId == usuarioId"), "Recurring repository should filter occurrences by user");
            TestAssert.True(source.Contains("MesReferencia == mes"), "Recurring repository should query suggestions by normalized month");

            return Task.CompletedTask;
        }
    }
}
