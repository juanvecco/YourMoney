using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class InvestimentoOrganizacaoConsultaControllerTests
{
    public static async Task BindsEveryConsolidatedFilter()
    {
        var fake = new FakeInvestimentoService();
        var controller = new InvestimentoController(fake);
        var group = Guid.NewGuid();
        TestAssert.True(await controller.ObterConsolidado(group, false, "Acoes", false) is OkObjectResult, "Valid filters should return 200");
        TestAssert.Equal(group, fake.LastFilter?.GrupoObjetivoId, "Group filter should bind");
        TestAssert.Equal("Acoes", fake.LastFilter?.TipoAtivo, "Asset filter should bind");
    }

    public static async Task MapsInvalidUnavailableAndUnexpectedQueries()
    {
        var fake = new FakeInvestimentoService { ConsolidatedException = new ArgumentException() };
        var controller = new InvestimentoController(fake);
        TestAssert.True(await controller.ObterConsolidado() is BadRequestObjectResult, "Invalid filter should return 400");
        fake.ConsolidatedException = new KeyNotFoundException();
        TestAssert.True(await controller.ObterConsolidado() is NotFoundObjectResult, "Unavailable group should return 404");
        fake.ConsolidatedException = new Exception();
        TestAssert.Equal(500, (await controller.ObterConsolidado() as ObjectResult)?.StatusCode, "Unexpected query failure should return 500");
    }
}
