using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Application.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class InvestimentoReservaControllerTests
    {
        public static async Task ConsolidatedEndpointReturnsTypedEmptyContract()
        {
            var response = new CarteiraInvestimentosConsolidadaResponse();
            var controller = new InvestimentoController(new FakeInvestimentoService { ConsolidatedResponse = response });
            var result = await controller.ObterConsolidado() as OkObjectResult;
            TestAssert.Equal(response, result?.Value as CarteiraInvestimentosConsolidadaResponse, "Consolidated endpoint should return typed empty contract");
        }

        public static async Task CreateReturnsConflictForReusedOperation()
        {
            var controller = new InvestimentoController(new FakeInvestimentoService { CreateException = new ConflitoOperacaoInvestimentoException() });
            var result = await controller.AdicionarInvestimento(InvestimentoTestFixtures.CriarRequest());
            TestAssert.True(result is ConflictObjectResult, "Reused operation with different data should return 409");
        }

        public static Task EndpointsRemainAuthorized()
        {
            var authorized = typeof(InvestimentoController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            TestAssert.True(authorized, "Investment endpoints should require authorization");
            return Task.CompletedTask;
        }
    }
}
