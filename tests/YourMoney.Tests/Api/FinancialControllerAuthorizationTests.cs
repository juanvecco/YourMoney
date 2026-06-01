using Microsoft.AspNetCore.Authorization;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class FinancialControllerAuthorizationTests
    {
        public static Task FinancialControllersRequireAuthorization()
        {
            var controllerTypes = new[]
            {
                typeof(CategoriaController),
                typeof(ContaFinanceiraController),
                typeof(DashboardController),
                typeof(DespesasController),
                typeof(InvestimentoController),
                typeof(ReceitasController)
            };

            foreach (var controllerType in controllerTypes)
            {
                var hasAuthorize = controllerType
                    .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                    .Any();

                TestAssert.True(hasAuthorize, $"{controllerType.Name} should require authorization at class level");
            }

            return Task.CompletedTask;
        }
    }
}
