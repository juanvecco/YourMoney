using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;
public static class AporteObjetivoInvestimentoControllerTests
{
    public static Task ExposesHistoryConfirmationAndAllocationRoutes()
    {
        foreach (var method in new[] { "ListarAportes", "ConfirmarAporte", "AlocarAporte" })
            TestAssert.True(typeof(MetasController).GetMethod(method)?.GetCustomAttributes(true).Any(a => a is HttpGetAttribute || a is HttpPostAttribute || a is HttpPutAttribute) == true, $"{method} route must exist");
        return Task.CompletedTask;
    }
}
