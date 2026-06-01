using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class DashboardIsolationTests
    {
        public static Task DashboardUsesCurrentUserForAllAggregations()
        {
            var source = File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Application", "Services", "DashboardService.cs"));

            TestAssert.True(source.Contains("_currentUserService.UserId"), "Dashboard service should derive the owner from the authenticated user");
            TestAssert.True(source.Contains("_receitaRepository.GetByMesAnoAsync(mes, ano, usuarioId)"), "Dashboard summary should scope receitas by user");
            TestAssert.True(source.Contains("_despesaRepository.GetByMesAnoAsync(mes, ano, usuarioId)"), "Dashboard summary should scope despesas by user");
            TestAssert.True(source.Contains("_investimentoRepository.GetAllAsync(usuarioId)"), "Dashboard summary should scope investimentos by user");
            TestAssert.True(source.Contains("_metaRepository.GetAtivasAsync(usuarioId)"), "Dashboard summary should scope metas by user");
            TestAssert.True(source.Contains("_despesaRepository.GetByMesAnoAsync(mes, ano, _currentUserService.UserId)"), "Dashboard expense chart should scope by current user");
            TestAssert.True(source.Contains("_receitaRepository.GetByMesAnoAsync(mes, ano, _currentUserService.UserId)"), "Dashboard revenue chart should scope by current user");
            return Task.CompletedTask;
        }

        public static Task DashboardDoesNotUseUnscopedRepositoryCalls()
        {
            var source = File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Application", "Services", "DashboardService.cs"));

            TestAssert.True(!source.Contains("GetAllAsync()"), "Dashboard service should not use unscoped GetAllAsync");
            TestAssert.True(!source.Contains("ListarAsync()"), "Dashboard service should not use unscoped ListarAsync");
            return Task.CompletedTask;
        }
    }
}
