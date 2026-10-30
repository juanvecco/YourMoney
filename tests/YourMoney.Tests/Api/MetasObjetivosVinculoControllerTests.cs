using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;
public static class MetasObjetivosVinculoControllerTests
{
    public static Task CommandsStayProtectedAndTyped()
    {
        TestAssert.True(typeof(MetasController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any(), "Metas must require authorization");
        TestAssert.True(typeof(InvestimentoGruposController).GetMethod("Criar")?.GetCustomAttributes(typeof(HttpPostAttribute), true).Any() == true, "Objective create endpoint must be POST");
        return Task.CompletedTask;
    }
}
