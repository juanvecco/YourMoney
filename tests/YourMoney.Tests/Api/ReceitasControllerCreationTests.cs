using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class ReceitasControllerCreationTests
    {
        public static async Task PostReturnsCreatedTypedContract()
        {
            var response = new CriarReceitaResponse
            {
                Id = Guid.NewGuid(),
                Descricao = "Salário",
                Valor = 5250.75m,
                Data = new DateTime(2026, 6, 5),
                MesReferencia = new DateTime(2026, 5, 1)
            };
            var service = new FakeReceitaService { CreateResponse = response };

            var result = await new ReceitasController(service).AdicionarReceita(ReceitaTestFixtures.CriarRequest());

            var created = result as CreatedAtActionResult;
            TestAssert.True(created != null, "POST should return CreatedAtActionResult");
            TestAssert.Equal(nameof(ReceitasController.ObterPorReferencia), created!.ActionName, "POST should link to monthly query");
            TestAssert.Equal(response, created.Value, "POST should return typed response");
            TestAssert.True(service.LastRequest != null, "POST should pass typed request");
        }

        public static async Task PostReturnsValidationMessage()
        {
            var service = new FakeReceitaService { CreateException = new ArgumentException("Valor deve ser maior que zero.") };
            var result = await new ReceitasController(service).AdicionarReceita(ReceitaTestFixtures.CriarRequest());
            TestAssert.True(result is BadRequestObjectResult, "Validation should return 400");
        }

        public static async Task PostReturnsSafeUnexpectedFailure()
        {
            var service = new FakeReceitaService { CreateException = new InvalidOperationException("Sensitive database detail.") };
            var result = await new ReceitasController(service).AdicionarReceita(ReceitaTestFixtures.CriarRequest());
            var objectResult = result as ObjectResult;
            TestAssert.Equal(500, objectResult?.StatusCode ?? 0, "Unexpected failure should return 500");
            TestAssert.True(!objectResult!.Value!.ToString()!.Contains("Sensitive"), "Unexpected failure should hide internal detail");
        }

        public static Task PostRequiresAuthorization()
        {
            var hasAuthorize = typeof(ReceitasController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            TestAssert.True(hasAuthorize, "Receitas controller should require authorization");
            return Task.CompletedTask;
        }
    }
}
