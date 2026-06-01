using Microsoft.AspNetCore.Authorization;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class FinancialDataAfterLoginAuthorizationTests
    {
        public static Task FinancialControllersStayProtectedAfterLoginFix()
        {
            foreach (var controller in new[]
            {
                typeof(DespesasController),
                typeof(ReceitasController),
                typeof(InvestimentoController),
                typeof(DashboardController),
                typeof(ContaFinanceiraController),
                typeof(CategoriaController)
            })
            {
                var hasAuthorize = controller
                    .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                    .Any();

                TestAssert.True(hasAuthorize, $"{controller.Name} should require an authenticated user");
            }

            return Task.CompletedTask;
        }
    }
}
