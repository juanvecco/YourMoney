using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class FinancialDataAfterLoginTests
    {
        public static Task FinancialQueriesUseAuthenticatedUserForPeriodData()
        {
            var serviceRoot = RepositoryTestPaths.InYourMoney("src", "YourMoney.Application", "Services");
            var expectations = new Dictionary<string, string[]>
            {
                ["DespesaService.cs"] = new[] { "ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId" },
                ["ReceitaService.cs"] = new[] { "ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId" },
                ["InvestimentoService.cs"] = new[] { "ObterPorMesAnoAsync(mes, ano, _currentUserService.UserId" },
                ["DashboardService.cs"] = new[] { "_currentUserService.UserId" }
            };

            foreach (var expectation in expectations)
            {
                var source = File.ReadAllText(Path.Combine(serviceRoot, expectation.Key));

                foreach (var expectedSnippet in expectation.Value)
                    TestAssert.True(source.Contains(expectedSnippet), $"{expectation.Key} should use authenticated user for period data");
            }

            return Task.CompletedTask;
        }

        public static Task FinancialServicesDoNotMutateExistingOwnershipForReads()
        {
            var serviceRoot = RepositoryTestPaths.InYourMoney("src", "YourMoney.Application", "Services");

            foreach (var name in new[] { "DespesaService.cs", "ReceitaService.cs", "InvestimentoService.cs", "DashboardService.cs" })
            {
                var source = File.ReadAllText(Path.Combine(serviceRoot, name));

                TestAssert.True(!source.Contains("legacy-transition-user"), $"{name} should not depend on transition ownership");
                TestAssert.True(!source.Contains("TargetUserId"), $"{name} should not hardcode a target owner");
            }

            return Task.CompletedTask;
        }
    }
}
