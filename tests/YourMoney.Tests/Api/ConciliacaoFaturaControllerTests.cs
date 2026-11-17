using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Api.Services;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class ConciliacaoFaturaControllerTests
{
    public static async Task AuthorizesAndMapsValidationAndOwnership()
    {
        TestAssert.True(typeof(ConciliacaoFaturaController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).Any(), "Bill API must require auth");
        var rule = typeof(ConciliacaoFaturaController)
            .GetCustomAttributes(typeof(RegraNegocioAtivaAttribute), true)
            .Cast<RegraNegocioAtivaAttribute>().Single();
        TestAssert.Equal(RegraNegocioCatalogo.ConciliacaoFaturaCartao, (string)rule.Arguments!.Single()!,
            "Bill GET and PUT must require the reconciliation business rule");
        var review = typeof(DespesasController).GetMethod(nameof(DespesasController.DefinirConferencia))!
            .GetCustomAttributes(typeof(RegraNegocioAtivaAttribute), true)
            .Cast<RegraNegocioAtivaAttribute>().Single();
        TestAssert.Equal(RegraNegocioCatalogo.ConciliacaoFaturaCartao, (string)review.Arguments!.Single()!,
            "Expense review writes must require the same business rule");
        var fake = new FakeService();
        var controller = new ConciliacaoFaturaController(fake);
        var contaId = Guid.NewGuid();

        var valid = await controller.Salvar(contaId, new SalvarFaturaRequest { Mes = 11, Ano = 2026, ValorInformado = 0 });
        TestAssert.True(valid is OkObjectResult, "Zero bill must be accepted");
        fake.Error = new ArgumentException("Inválido");
        var invalid = await controller.Obter(contaId, 13, 2026);
        TestAssert.True(invalid is BadRequestObjectResult, "Invalid month must map to 400");
        fake.Error = new InvalidOperationException("Conta não encontrada");
        var foreign = await controller.Obter(contaId, 11, 2026);
        TestAssert.True(foreign is NotFoundObjectResult, "Foreign account must be hidden as 404");
    }

    private sealed class FakeService : IConciliacaoFaturaService
    {
        public Exception? Error { get; set; }
        public Task<ResumoFaturaResponse> ObterAsync(Guid contaId, int mes, int ano) =>
            Error is null ? Task.FromResult(new ResumoFaturaResponse { IdContaFinanceira = contaId, Mes = mes, Ano = ano })
                : Task.FromException<ResumoFaturaResponse>(Error);
        public Task<ResumoFaturaResponse> SalvarAsync(Guid contaId, int mes, int ano, decimal valor) =>
            ObterAsync(contaId, mes, ano);
    }
}
