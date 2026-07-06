using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class DespesasControllerConsultaTests
    {
        public static async Task ConsultaReturnsTypedPagedContract()
        {
            var response = new ConsultaDespesasResponse
            {
                Itens = new List<DespesaDTO>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Descricao = "Mercado",
                        Valor = 120m,
                        ValorLiquido = 120m,
                        Data = new DateTime(2026, 7, 4),
                        IdContaFinanceira = DespesaTestFixtures.ContaId,
                        IdCategoria = DespesaTestFixtures.CategoriaId
                    }
                },
                PaginaAtual = 1,
                TamanhoPagina = 10,
                TotalResultados = 1,
                TotalPaginas = 1,
                ValorTotalFiltrado = 120m,
                TotaisPorConta = new List<ConsultaDespesasTotalPorContaDTO>
                {
                    new() { IdContaFinanceira = DespesaTestFixtures.ContaId, Valor = 120m }
                }
            };
            var service = new FakeDespesaService { ConsultaResponse = response };
            var request = new ConsultaDespesasRequest
            {
                Mes = 7,
                Ano = 2026,
                IdContaFinanceira = DespesaTestFixtures.ContaId,
                Pagina = 1,
                TamanhoPagina = 10
            };

            var result = await new DespesasController(service).ConsultarDespesas(request);

            var ok = result as OkObjectResult;
            TestAssert.True(ok != null, "Consulta should return OK");
            TestAssert.Equal(response, ok!.Value, "Consulta should return typed response");
            TestAssert.Equal(DespesaTestFixtures.ContaId, service.LastConsultaDespesasRequest!.IdContaFinanceira, "Consulta should pass query params to service");
            TestAssert.Equal(120m, response.TotaisPorConta.Single().Valor, "Consulta should expose account distribution totals");
        }

        public static async Task ConsultaReturnsFilteredTotalForEmptyResponse()
        {
            var service = new FakeDespesaService
            {
                ConsultaResponse = new ConsultaDespesasResponse
                {
                    PaginaAtual = 1,
                    TamanhoPagina = 10,
                    TotalResultados = 0,
                    TotalPaginas = 0,
                    ValorTotalFiltrado = 0m
                }
            };

            var result = await new DespesasController(service).ConsultarDespesas(new ConsultaDespesasRequest { Mes = 7, Ano = 2026 });

            var ok = result as OkObjectResult;
            var body = ok?.Value as ConsultaDespesasResponse;
            TestAssert.True(body != null, "Empty consulta should return typed response");
            TestAssert.Equal(0m, body!.ValorTotalFiltrado, "Empty consulta should expose zero total");
            TestAssert.Equal(0, body.TotalResultados, "Empty consulta should expose zero result count");
        }

        public static async Task ConsultaReturnsGenericValidationError()
        {
            var service = new FakeDespesaService
            {
                ConsultaException = new ArgumentException("foreign filter")
            };

            var result = await new DespesasController(service).ConsultarDespesas(new ConsultaDespesasRequest { Mes = 7, Ano = 2026 });

            var badRequest = result as BadRequestObjectResult;
            TestAssert.True(badRequest != null, "Invalid consulta should return bad request");
            TestAssert.True(badRequest!.Value!.ToString()!.Contains("Filtros de despesa"), "Invalid consulta should return generic validation message");
        }
    }
}
