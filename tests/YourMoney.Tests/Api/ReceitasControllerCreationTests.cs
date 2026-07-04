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
                MesReferencia = new DateTime(2026, 5, 1),
                Natureza = "RendaDisponivel",
                ConsideraNasMetas = true,
                ValorAbatidoEmDespesa = 0m
            };
            var service = new FakeReceitaService { CreateResponse = response };

            var result = await new ReceitasController(service).AdicionarReceita(ReceitaTestFixtures.CriarRequest());

            var created = result as CreatedAtActionResult;
            TestAssert.True(created != null, "POST should return CreatedAtActionResult");
            TestAssert.Equal(nameof(ReceitasController.ObterPorReferencia), created!.ActionName, "POST should link to monthly query");
            TestAssert.Equal(response, created.Value, "POST should return typed response");
            TestAssert.True(service.LastRequest != null, "POST should pass typed request");
        }

        public static async Task PostPreservesClassificationContract()
        {
            var despesaId = Guid.NewGuid();
            var response = new CriarReceitaResponse
            {
                Id = Guid.NewGuid(),
                Descricao = "Reembolso compra para terceiro",
                Valor = 150m,
                Data = new DateTime(2026, 7, 5),
                MesReferencia = new DateTime(2026, 7, 1),
                Natureza = "Reembolso",
                ConsideraNasMetas = false,
                DespesaVinculadaId = despesaId,
                DespesaVinculadaDescricao = "Compra para terceiro",
                ValorAbatidoEmDespesa = 150m
            };
            var service = new FakeReceitaService { CreateResponse = response };

            var result = await new ReceitasController(service).AdicionarReceita(
                ReceitaTestFixtures.CriarRequest(
                    descricao: "Reembolso compra para terceiro",
                    valor: 150m,
                    natureza: "Reembolso",
                    despesaVinculadaId: despesaId));

            var created = result as CreatedAtActionResult;
            var body = created?.Value as CriarReceitaResponse;
            TestAssert.Equal("Reembolso", service.LastRequest?.Natureza, "POST should pass classification to service");
            TestAssert.Equal<Guid?>(despesaId, service.LastRequest?.DespesaVinculadaId, "POST should pass linked expense");
            TestAssert.True(body != null, "POST should return typed classification response");
            TestAssert.Equal("Reembolso", body!.Natureza, "Response should expose nature");
            TestAssert.True(!body.ConsideraNasMetas, "Response should expose goal impact");
            TestAssert.Equal<Guid?>(despesaId, body.DespesaVinculadaId, "Response should expose linked expense");
            TestAssert.Equal(150m, body.ValorAbatidoEmDespesa, "Response should expose reimbursed value");
        }

        public static async Task PutReturnsClassifiedReceitaContract()
        {
            var id = Guid.NewGuid();
            var dto = new ReceitaDTO
            {
                Id = id,
                Descricao = "Vale alimentação",
                Valor = 800m,
                Data = new DateTime(2026, 7, 1),
                MesReferencia = new DateTime(2026, 7, 1),
                Natureza = "EntradaVinculadaDespesa",
                ConsideraNasMetas = false,
                ValorAbatidoEmDespesa = 0m
            };

            var result = await new ReceitasController(new FakeReceitaService()).AtualizarReceita(id, dto);

            var ok = result as OkObjectResult;
            var body = ok?.Value as ReceitaDTO;
            TestAssert.True(body != null, "PUT should return typed receita response");
            TestAssert.Equal("EntradaVinculadaDespesa", body!.Natureza, "PUT response should expose updated nature");
            TestAssert.True(!body.ConsideraNasMetas, "PUT response should expose updated goal impact");
        }

        public static async Task PostReturnsValidationMessage()
        {
            var service = new FakeReceitaService { CreateException = new ArgumentException("Valor deve ser maior que zero.") };
            var result = await new ReceitasController(service).AdicionarReceita(ReceitaTestFixtures.CriarRequest());
            TestAssert.True(result is BadRequestObjectResult, "Validation should return 400");
        }

        public static async Task PostReturnsValidationForInvalidReimbursement()
        {
            var service = new FakeReceitaService { CreateException = new ArgumentException("Total de reembolsos não pode ultrapassar o valor pendente da despesa.") };
            var result = await new ReceitasController(service).AdicionarReceita(ReceitaTestFixtures.CriarRequest(natureza: "Reembolso", despesaVinculadaId: Guid.NewGuid()));
            TestAssert.True(result is BadRequestObjectResult, "Invalid reimbursement should return 400");
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
