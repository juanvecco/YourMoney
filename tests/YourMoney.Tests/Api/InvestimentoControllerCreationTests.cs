using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class InvestimentoControllerCreationTests
    {
        public static async Task PostReturnsCreatedTypedContract()
        {
            var response = new CriarInvestimentoResponse
            {
                Id = Guid.NewGuid(),
                Nome = "Tesouro",
                Tipo = "Renda fixa",
                Quantidade = 1,
                PrecoMedio = 100m,
                ValorAtual = 100m,
                DataInvestimento = new DateTime(2026, 6, 9),
                MesReferencia = new DateTime(2026, 5, 1),
                Ativo = true
            };
            var service = new FakeInvestimentoService { CreateResponse = response };
            var controller = new InvestimentoController(service);

            var result = await controller.AdicionarInvestimento(InvestimentoTestFixtures.CriarRequest());

            var created = result as CreatedAtActionResult;
            TestAssert.True(created != null, "POST should return CreatedAtActionResult");
            TestAssert.Equal(nameof(InvestimentoController.ObterPorReferencia), created!.ActionName, "POST should link to monthly query");
            TestAssert.Equal(response, created.Value, "POST should return typed response");
            var routeValues = created.RouteValues!;
            TestAssert.Equal(5, Convert.ToInt32(routeValues["mes"]), "Location should use reference month");
            TestAssert.Equal(2026, Convert.ToInt32(routeValues["ano"]), "Location should use reference year");
            TestAssert.True(service.LastRequest != null, "POST should pass typed request to service");
        }

        public static async Task PutReturnsPersistedContract()
        {
            var response = new InvestimentoResponse
            {
                Id = Guid.NewGuid(),
                Nome = "Tesouro",
                Tipo = "Renda fixa",
                MesReferencia = new DateTime(2026, 7, 1)
            };
            var service = new FakeInvestimentoService { UpdateResponse = response };
            var controller = new InvestimentoController(service);
            var request = new AtualizarInvestimentoRequest
            {
                Nome = "Tesouro",
                Descricao = "",
                Tipo = "Renda fixa",
                Quantidade = 1,
                PrecoMedio = 100,
                ValorAtual = 110,
                DataInvestimento = new DateTime(2026, 6, 9),
                MesReferencia = new DateTime(2026, 7, 10)
            };

            var result = await controller.AtualizarInvestimento(response.Id, request);

            var ok = result as OkObjectResult;
            TestAssert.Equal(response, ok?.Value, "PUT should return persisted response");
            TestAssert.Equal(request, service.LastUpdateRequest, "PUT should pass typed request");
        }

        public static async Task PutReturnsSafeErrorContracts()
        {
            var request = new AtualizarInvestimentoRequest
            {
                Nome = "Tesouro",
                Descricao = "",
                Tipo = "Renda fixa",
                Quantidade = 1,
                PrecoMedio = 100,
                ValorAtual = 110,
                DataInvestimento = new DateTime(2026, 6, 9),
                MesReferencia = new DateTime(2026, 7, 1)
            };

            var validation = await new InvestimentoController(new FakeInvestimentoService
            {
                UpdateException = new ArgumentException("Mês de referência é obrigatório.")
            }).AtualizarInvestimento(Guid.NewGuid(), request);
            TestAssert.True(validation is BadRequestObjectResult, "Invalid update should return 400");

            var missing = await new InvestimentoController(new FakeInvestimentoService
            {
                UpdateException = new InvalidOperationException("Investimento não encontrado.")
            }).AtualizarInvestimento(Guid.NewGuid(), request);
            TestAssert.True(missing is NotFoundObjectResult, "Missing owner-scoped update should return 404");

            var failure = await new InvestimentoController(new FakeInvestimentoService
            {
                UpdateException = new Exception("Sensitive detail.")
            }).AtualizarInvestimento(Guid.NewGuid(), request);
            var objectResult = failure as ObjectResult;
            TestAssert.Equal(500, objectResult?.StatusCode ?? 0, "Unexpected update failure should return 500");
            TestAssert.True(!objectResult!.Value!.ToString()!.Contains("Sensitive"), "Unexpected update failure should hide detail");
        }

        public static async Task PostReturnsValidationMessage()
        {
            var service = new FakeInvestimentoService { CreateException = new ArgumentException("Quantidade deve ser maior que zero.") };
            var result = await new InvestimentoController(service).AdicionarInvestimento(InvestimentoTestFixtures.CriarRequest());
            TestAssert.True(result is BadRequestObjectResult, "Validation should return 400");
        }

        public static async Task PostReturnsSafeUnexpectedFailure()
        {
            var service = new FakeInvestimentoService { CreateException = new InvalidOperationException("Sensitive database detail.") };
            var result = await new InvestimentoController(service).AdicionarInvestimento(InvestimentoTestFixtures.CriarRequest());
            var objectResult = result as ObjectResult;
            TestAssert.Equal(500, objectResult?.StatusCode ?? 0, "Unexpected failure should return 500");
            TestAssert.True(!objectResult!.Value!.ToString()!.Contains("Sensitive"), "Unexpected failure should hide internal detail");
        }

        public static Task PostRequiresAuthorization()
        {
            var hasAuthorize = typeof(InvestimentoController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            TestAssert.True(hasAuthorize, "Investment controller should require authorization");
            return Task.CompletedTask;
        }
    }
}
