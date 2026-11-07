using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class RegrasNegocioControllerTests
{
    public static async Task RequiresAuthenticationAndValidatesUpdates()
    {
        TestAssert.True(Attribute.IsDefined(typeof(RegrasNegocioController), typeof(AuthorizeAttribute)),
            "Rule preferences contain private account data and require authorization");
        var controller = new RegrasNegocioController(new FakeRules());
        TestAssert.True(await controller.Atualizar("despesas.recorrencia", new AtualizarRegraNegocioRequest())
            is BadRequestObjectResult, "Omitted state must not silently turn a rule off");
        TestAssert.True(await controller.Atualizar("unknown", new AtualizarRegraNegocioRequest { Ativa = false })
            is NotFoundObjectResult, "Unknown rules must return 404");
        TestAssert.True(await controller.Atualizar("despesas.recorrencia", new AtualizarRegraNegocioRequest { Ativa = false })
            is OkObjectResult, "Valid update must return the saved rule");
    }

    private sealed class FakeRules : IRegraNegocioService
    {
        public Task<IReadOnlyList<RegraNegocioDto>> ListarAsync() =>
            Task.FromResult<IReadOnlyList<RegraNegocioDto>>(Array.Empty<RegraNegocioDto>());

        public Task<RegraNegocioDto> AtualizarAsync(string codigo, bool ativa)
        {
            if (codigo == "unknown") throw new KeyNotFoundException();
            return Task.FromResult(new RegraNegocioDto(codigo, "Recorrência", "Mensal", "Despesas", ativa, ativa, Array.Empty<string>()));
        }

        public Task<bool> EstaAtivaAsync(string codigo) => Task.FromResult(true);
    }
}
