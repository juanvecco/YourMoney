using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class FinancialServiceIsolationTests
    {
        public static Task ServicesUseCurrentUserForFinancialOperations()
        {
            foreach (var file in ServiceFiles())
            {
                var source = File.ReadAllText(file);
                var name = Path.GetFileName(file);

                TestAssert.True(source.Contains("_currentUserService.UserId"), $"{name} should use the authenticated user id");
                TestAssert.True(!source.Contains(FinancialOwnershipTestConstants.LegacyTransitionUserId), $"{name} should not use the transition user id");
            }

            return Task.CompletedTask;
        }

        public static Task ServicesDoNotExposeClientSuppliedOwners()
        {
            foreach (var file in ServiceFiles())
            {
                var source = File.ReadAllText(file);
                var name = Path.GetFileName(file);

                TestAssert.True(!source.Contains("dto.UsuarioId", StringComparison.OrdinalIgnoreCase), $"{name} should not trust a client-supplied UsuarioId");
                TestAssert.True(!source.Contains("request.UsuarioId", StringComparison.OrdinalIgnoreCase), $"{name} should not trust a request-supplied UsuarioId");
            }

            return Task.CompletedTask;
        }

        private static IEnumerable<string> ServiceFiles()
        {
            var serviceRoot = RepositoryTestPaths.InYourMoney("src", "YourMoney.Application", "Services");
            return new[]
            {
                "CategoriaService.cs",
                "ContaFinanceiraService.cs",
                "DespesaService.cs",
                "InvestimentoService.cs",
                "ReceitaService.cs",
                "DashboardService.cs"
            }.Select(name => Path.Combine(serviceRoot, name));
        }
    }
}
