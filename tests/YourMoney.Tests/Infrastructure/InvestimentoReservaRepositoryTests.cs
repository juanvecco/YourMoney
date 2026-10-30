using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class InvestimentoReservaRepositoryTests
    {
        public static Task MappingDefinesOptionalRestrictedRelationshipAndIndexes()
        {
            var configuration = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Configurations", "InvestimentoConfiguration.cs"));
            var migrationsDirectory = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Migrations");
            var migrationPath = Directory.GetFiles(migrationsDirectory, "*_AddInvestimentosReservaSalarial.cs")
                .Single();
            var migration = File.ReadAllText(migrationPath);

            TestAssert.True(configuration.Contains("ReceitaRecorrenteId"), "Investment mapping should define recurring salary relationship");
            TestAssert.True(configuration.Contains("DeleteBehavior.Restrict"), "Salary relationship should restrict deletes");
            TestAssert.True(configuration.Contains("IX_Investimento_UsuarioId_DataInvestimento"), "Mapping should index owner and date");
            TestAssert.True(configuration.Contains("IX_Investimento_UsuarioId_ReceitaRecorrenteId"), "Mapping should index owner and salary");
            TestAssert.True(configuration.Contains("UX_Investimento_UsuarioId_OperacaoId"), "Mapping should define unique operation index");
            TestAssert.True(migration.Contains("nullable: true"), "New columns should preserve legacy rows");
            TestAssert.True(!migration.Contains("UpdateData"), "Migration should not infer historical associations");
            return Task.CompletedTask;
        }

        public static Task ConsolidatedQueriesAreSetBasedAndOwnerScoped()
        {
            var investmentRepository = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "InvestimentoRepository.cs"));
            var salaryRepository = File.ReadAllText(RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "ReceitaRecorrenteRepository.cs"));
            TestAssert.True(investmentRepository.Contains("AsNoTracking()"), "Consolidated investment read should be untracked");
            TestAssert.True(investmentRepository.Contains("ThenInclude(r => r!.ContaFinanceira)"), "Consolidated read should eager-load salary account without N+1");
            TestAssert.True(investmentRepository.Contains("Where(i => i.UsuarioId == usuarioId)"), "Consolidated read should be owner scoped");
            TestAssert.True(salaryRepository.Contains("ListarReservasSalariaisAtivasAsync"), "Reserve salary lookup should be set based");
            TestAssert.True(!investmentRepository.Contains("foreach"), "Consolidated repository should not issue per-row queries");
            return Task.CompletedTask;
        }
    }
}
