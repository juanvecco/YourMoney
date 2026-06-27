using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class MetasControllerTests
    {
        public static async Task GetResumoReturnsTypedContract()
        {
            var service = new FakeMetaMensalService
            {
                SummaryResponse = new MetasMensaisResumoDTO
                {
                    MesReferencia = new DateTime(2026, 6, 1),
                    ReceitaTotal = 10000m,
                    Status = "disponivel"
                }
            };

            var result = await new MetasController(service).ObterResumo(6, 2026);

            var ok = result as OkObjectResult;
            TestAssert.True(ok != null, "GET resumo should return 200");
            TestAssert.Equal(service.SummaryResponse, ok!.Value, "GET resumo should return typed summary");
        }

        public static async Task PostPutDeleteReturnExpectedContracts()
        {
            var metaId = Guid.NewGuid();
            var service = new FakeMetaMensalService
            {
                MetaResponse = new MetaMensalDTO
                {
                    Id = metaId,
                    Nome = "Investimento",
                    PercentualReceita = 25m,
                    ValorCalculado = 2500m,
                    MesReferencia = new DateTime(2026, 6, 1)
                }
            };
            var controller = new MetasController(service);

            var post = await controller.Criar(MetaMensalTestFixtures.CriarRequest());
            var created = post as CreatedAtActionResult;
            TestAssert.True(created != null, "POST should return CreatedAtAction");
            TestAssert.Equal(nameof(MetasController.ObterResumo), created!.ActionName, "POST should link to summary");

            var put = await controller.Atualizar(metaId, new AtualizarMetaMensalDTO
            {
                Id = metaId,
                Nome = "Investimento",
                PercentualReceita = 25m
            });
            TestAssert.True(put is OkObjectResult, "PUT should return 200");

            var delete = await controller.Remover(metaId);
            TestAssert.True(delete is NoContentResult, "DELETE should return 204");
            TestAssert.Equal(metaId, service.LastDeletedId!.Value, "DELETE should pass route id");
        }

        public static async Task ReturnsValidationAndNotFoundContracts()
        {
            var badRequest = await new MetasController(new FakeMetaMensalService
            {
                Exception = new ArgumentException("Percentual inválido.")
            }).Criar(MetaMensalTestFixtures.CriarRequest());
            TestAssert.True(badRequest is BadRequestObjectResult, "Validation should return 400");

            var notFound = await new MetasController(new FakeMetaMensalService
            {
                Exception = new InvalidOperationException("Meta não encontrada.")
            }).Atualizar(Guid.NewGuid(), new AtualizarMetaMensalDTO { Id = Guid.NewGuid(), Nome = "Meta", PercentualReceita = 1m });
            TestAssert.True(notFound is NotFoundObjectResult, "Missing meta should return 404 on update");
        }

        public static Task RequiresAuthorization()
        {
            var hasAuthorize = typeof(MetasController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            TestAssert.True(hasAuthorize, "Metas controller should require authorization");
            return Task.CompletedTask;
        }
    }
}
