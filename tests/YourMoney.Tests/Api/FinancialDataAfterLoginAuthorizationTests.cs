using Microsoft.AspNetCore.Authorization;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class FinancialDataAfterLoginAuthorizationTests
    {
        public static Task FinancialControllersStayProtectedForLocalFrontendOrigins()
        {
            foreach (var controller in FinancialControllers())
            {
                var hasAuthorize = controller
                    .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                    .Any();

                TestAssert.True(hasAuthorize, $"{controller.Name} should remain protected regardless of the local frontend origin");
            }

            return Task.CompletedTask;
        }

        public static Task FinancialControllersStayProtectedAfterLoginFix()
        {
            foreach (var controller in FinancialControllers())
            {
                var hasAuthorize = controller
                    .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                    .Any();

                TestAssert.True(hasAuthorize, $"{controller.Name} should require an authenticated user");
            }

            return Task.CompletedTask;
        }

        private static Type[] FinancialControllers() =>
        [
            typeof(DespesasController),
            typeof(ReceitasController),
            typeof(InvestimentoController),
            typeof(DashboardController),
            typeof(ContaFinanceiraController),
            typeof(CategoriaController)
        ];
    }
}
