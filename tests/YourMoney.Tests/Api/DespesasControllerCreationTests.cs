using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class DespesasControllerCreationTests
    {
        public static async Task PostDespesaReturnsCreatedContract()
        {
            var response = new CriarDespesaResponse
            {
                Id = Guid.NewGuid(),
                Descricao = "Mercado",
                Valor = 120.35m,
                Data = new DateTime(2026, 5, 30),
                MesReferencia = new DateTime(2026, 5, 1),
                IdContaFinanceira = DespesaTestFixtures.ContaId,
                IdCategoria = DespesaTestFixtures.CategoriaId
            };
            var service = new FakeDespesaService { CriarDespesaResponse = response };
            var controller = new DespesasController(service);

            var result = await controller.AdicionarDespesa(DespesaTestFixtures.CriarRequest());

            var created = result as CreatedAtActionResult;
            TestAssert.True(created != null, "POST despesa should return CreatedAtActionResult");
            TestAssert.Equal(nameof(DespesasController.GetDespesaById), created!.ActionName, "POST despesa should link to get by id");
            TestAssert.Equal(response, created.Value, "POST despesa should return typed created response");
            TestAssert.True(service.LastCriarDespesaRequest != null, "POST despesa should pass typed request to service");
        }

        public static Task PostDespesaEndpointRequiresAuthorization()
        {
            var controllerHasAuthorize = typeof(DespesasController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Any();

            TestAssert.True(controllerHasAuthorize, "POST despesa should be protected by controller authorization");
            return Task.CompletedTask;
        }
    }
}
