using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class DespesasControllerParcelamentoTests
    {
        public static async Task PostParcelamentoReturnsCreatedContract()
        {
            var response = new ParcelamentoDespesaResponse
            {
                ParcelamentoId = Guid.NewGuid(),
                ValorTotal = 100m,
                QuantidadeParcelas = 3,
                Parcelas = new List<ParcelaDespesaResponse>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Descricao = "Notebook",
                        Valor = 33.34m,
                        Data = new DateTime(2026, 5, 26),
                        IdContaFinanceira = DespesaTestFixtures.ContaId,
                        IdCategoria = DespesaTestFixtures.CategoriaId,
                        ParcelamentoId = Guid.NewGuid(),
                        NumeroParcela = 1,
                        TotalParcelas = 3,
                        ValorTotalParcelamento = 100m
                    }
                }
            };
            var service = new FakeDespesaService { ParcelamentoResponse = response };
            var controller = new DespesasController(service);

            var result = await controller.CriarParcelamento(DespesaTestFixtures.Request());

            var created = result as CreatedAtActionResult;
            TestAssert.True(created != null, "POST parcelamento should return CreatedAtActionResult");
            TestAssert.Equal(response, created!.Value, "POST parcelamento should return service response");
        }

        public static async Task MonthlyQueryReturnsOkContract()
        {
            var queryResponse = new List<DespesaDTO>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Descricao = "Notebook",
                    Valor = 33.33m,
                    Data = new DateTime(2026, 6, 26),
                    IdContaFinanceira = DespesaTestFixtures.ContaId,
                    IdCategoria = DespesaTestFixtures.CategoriaId,
                    ParcelamentoId = Guid.NewGuid(),
                    NumeroParcela = 2,
                    TotalParcelas = 3,
                    ValorTotalParcelamento = 100m
                }
            };
            var service = new FakeDespesaService { QueryResponse = queryResponse };
            var controller = new DespesasController(service);

            var result = await controller.ObterPorReferencia(6, 2026, null);

            var ok = result as OkObjectResult;
            TestAssert.True(ok != null, "Monthly query should return OkObjectResult");
            TestAssert.Equal(queryResponse, ok!.Value, "Monthly query should return service response");
        }

        public static Task ParcelamentoEndpointRequiresAuthorization()
        {
            var hasAuthorize = typeof(DespesasController)
                .GetMethod(nameof(DespesasController.CriarParcelamento))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Any();

            TestAssert.True(hasAuthorize, "POST parcelamento should require authorization");
            return Task.CompletedTask;
        }
    }
}
