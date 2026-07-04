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

        public static async Task ListDespesaReturnsReimbursementAwareContract()
        {
            var response = new DespesaDTO
            {
                Id = Guid.NewGuid(),
                Descricao = "Compra para terceiro",
                Valor = 150m,
                Data = new DateTime(2026, 7, 4),
                IdContaFinanceira = DespesaTestFixtures.ContaId,
                IdCategoria = DespesaTestFixtures.CategoriaId,
                ValorReembolsado = 150m,
                ValorLiquido = 0m,
                PossuiReembolso = true
            };
            var service = new FakeDespesaService { QueryResponse = new List<DespesaDTO> { response } };

            var result = await new DespesasController(service).ObterPorReferencia(7, 2026, null);

            var ok = result as OkObjectResult;
            var body = ok?.Value as List<DespesaDTO>;
            TestAssert.True(body != null && body.Count == 1, "GET despesas should return typed list");
            TestAssert.Equal(150m, body![0].Valor, "Response should keep gross expense");
            TestAssert.Equal(150m, body[0].ValorReembolsado, "Response should expose reimbursed total");
            TestAssert.Equal(0m, body[0].ValorLiquido, "Response should expose liquid expense");
            TestAssert.True(body[0].PossuiReembolso, "Response should flag reimbursement");
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
