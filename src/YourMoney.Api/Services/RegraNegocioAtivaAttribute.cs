using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YourMoney.Application.Interfaces;

namespace YourMoney.Api.Services;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RegraNegocioAtivaAttribute : TypeFilterAttribute
{
    public RegraNegocioAtivaAttribute(string codigo) : base(typeof(RegraNegocioAtivaFilter))
    {
        Arguments = new object[] { codigo };
    }
}

public sealed class RegraNegocioAtivaFilter : IAsyncActionFilter
{
    private readonly string _codigo;
    private readonly IRegraNegocioService _rules;

    public RegraNegocioAtivaFilter(string codigo, IRegraNegocioService rules)
    {
        _codigo = codigo;
        _rules = rules;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!await _rules.EstaAtivaAsync(_codigo))
        {
            context.Result = new ObjectResult(new { message = "Esta função está desativada em Regras de negócio." })
            { StatusCode = StatusCodes.Status403Forbidden };
            return;
        }
        await next();
    }
}
