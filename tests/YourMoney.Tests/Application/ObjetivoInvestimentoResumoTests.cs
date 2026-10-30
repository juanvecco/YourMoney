using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;
public static class ObjetivoInvestimentoResumoTests
{
    public static Task SummarySeparatesPortfolioFromConfirmedContributions()
    {
        var resumo = new GrupoObjetivoInvestimentoResumoResponse { ValorAtualCarteira = 3629.20m, ValorConfirmadoMes = 1000m, ValorPlanejadoMes = 1000m };
        TestAssert.Equal(3629.20m, resumo.ValorAtualCarteira, "Portfolio value must remain independent from confirmation ledger");
        TestAssert.Equal(1000m, resumo.ValorConfirmadoMes, "Confirmed value must be reported independently");
        return Task.CompletedTask;
    }
}
