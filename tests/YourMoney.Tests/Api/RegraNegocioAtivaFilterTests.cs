using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using YourMoney.Api.Services;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class RegraNegocioAtivaFilterTests
{
    public static async Task RejectsDisabledActionBeforeMutation()
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var executing = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
            new Dictionary<string, object?>(), new object());
        var nextCalled = false;
        var filter = new RegraNegocioAtivaFilter(RegraNegocioCatalogo.DespesasRecorrentes, new FakeRules(false));

        await filter.OnActionExecutionAsync(executing, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        });

        TestAssert.True(!nextCalled, "Disabled feature must not reach the controller action");
        TestAssert.Equal(403, ((ObjectResult)executing.Result!).StatusCode,
            "Disabled feature must return forbidden");
    }

    public static async Task AllowsEnabledAction()
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var executing = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
            new Dictionary<string, object?>(), new object());
        var nextCalled = false;
        var filter = new RegraNegocioAtivaFilter(RegraNegocioCatalogo.ReceitasRecorrentes, new FakeRules(true));

        await filter.OnActionExecutionAsync(executing, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        });

        TestAssert.True(nextCalled, "Enabled feature must reach the action");
    }

    private sealed class FakeRules(bool active) : IRegraNegocioService
    {
        public Task<bool> EstaAtivaAsync(string codigo) => Task.FromResult(active);
        public Task<IReadOnlyList<RegraNegocioDto>> ListarAsync() =>
            Task.FromResult<IReadOnlyList<RegraNegocioDto>>(Array.Empty<RegraNegocioDto>());
        public Task<RegraNegocioDto> AtualizarAsync(string codigo, bool ativa) => throw new NotSupportedException();
    }
}
