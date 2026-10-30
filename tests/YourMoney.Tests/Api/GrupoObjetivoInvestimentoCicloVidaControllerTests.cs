using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;
public static class GrupoObjetivoInvestimentoCicloVidaControllerTests
{
    public static Task LifecycleEndpointsArePostCommands()
    {
        foreach (var method in new[] { "Arquivar", "Reativar" })
            TestAssert.True(typeof(InvestimentoGruposController).GetMethod(method)?.GetCustomAttributes(typeof(HttpPostAttribute), true).Any() == true, $"{method} endpoint must be POST");
        return Task.CompletedTask;
    }
}
