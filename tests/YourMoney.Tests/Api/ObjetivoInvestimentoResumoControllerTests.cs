using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;
public static class ObjetivoInvestimentoResumoControllerTests
{
    public static Task SummaryEndpointUsesTypedQueryContract()
    {
        var method = typeof(InvestimentoGruposController).GetMethod("Resumo");
        TestAssert.True(method?.GetCustomAttributes(typeof(HttpGetAttribute), true).Any() == true, "Summary GET endpoint must exist");
        TestAssert.Equal(3, method!.GetParameters().Length, "Summary must bind month, year and archive flag");
        return Task.CompletedTask;
    }
}
