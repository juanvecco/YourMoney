using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain;

public static class MetasObjetivosIntegracaoDomainTests
{
    public static Task ObjectiveSupportsTargetAndLifecycle()
    {
        var grupo = new GrupoObjetivoInvestimento("Aluguel para nós mesmos", "casal", 150000m, new DateTime(2034, 10, 1));
        TestAssert.Equal(150000m, grupo.ValorAlvo, "Target should be preserved");
        grupo.Arquivar();
        TestAssert.True(!grupo.Ativo && grupo.ArquivadoEm.HasValue, "Archive should preserve the objective");
        grupo.Reativar();
        TestAssert.True(grupo.Ativo && grupo.ArquivadoEm == null, "Reactivation should restore active state");
        return Task.CompletedTask;
    }

    public static Task MonthlyGoalCanLinkCanonicalObjective()
    {
        var grupo = new GrupoObjetivoInvestimento("Carro", "casal", 150000m);
        var meta = new MetaMensal("Guardar aluguel", TipoDefinicaoMeta.Valor, null, 1000m, new DateTime(2026, 10, 1), "casal");
        meta.AssociarGrupoObjetivo(grupo.Id);
        TestAssert.Equal(grupo.Id, meta.GrupoObjetivoInvestimentoId, "Goal should reference the canonical objective");
        return Task.CompletedTask;
    }

    public static async Task ContributionIsPreciseAndAllocationIsStable()
    {
        var aporte = new AporteObjetivoInvestimento("casal", Guid.NewGuid(), Guid.NewGuid(), 600.25m,
            new DateTime(2026, 10, 1), new DateTime(2026, 10, 26), Guid.NewGuid());
        var investimento = Guid.NewGuid();
        aporte.Alocar(investimento);
        TestAssert.Equal(600.25m, aporte.Valor, "Contribution must preserve cents");
        TestAssert.Equal(investimento, aporte.InvestimentoId, "Destination should be recorded without changing portfolio value");
        await TestAssert.ThrowsAsync<InvalidOperationException>(() => { aporte.Alocar(Guid.NewGuid()); return Task.CompletedTask; },
            "Confirmed allocation should be immutable");
    }
}
