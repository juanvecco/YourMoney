using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain;

public static class InvestimentoOrganizacaoDomainTests
{
    public static Task NormalizesAndLimitsGroupNames()
    {
        var grupo = new GrupoObjetivoInvestimento("  Reserva de Emergência  ", "user-a");
        TestAssert.Equal("Reserva de Emergência", grupo.Nome, "Group should trim display name");
        TestAssert.Equal("RESERVA DE EMERGÊNCIA", grupo.NomeNormalizado, "Group should normalize invariant uppercase");
        return TestAssert.ThrowsAsync<ArgumentException>(
            () => Task.FromResult(new GrupoObjetivoInvestimento(new string('x', 101), "user-a")),
            "Group name above 100 characters should fail");
    }

    public static Task KeepsOrganizationOptionalAndControlled()
    {
        var investimento = InvestimentoOrganizacaoTestFixtures.Investimento();
        TestAssert.True(investimento.GrupoObjetivoInvestimentoId == null, "Historical investment should have no group");
        TestAssert.True(investimento.TipoAtivo == null, "Historical investment should be unclassified");
        investimento.ClassificarTipoAtivo(TipoAtivo.Criptomoeda);
        TestAssert.Equal(TipoAtivo.Criptomoeda, investimento.TipoAtivo, "Controlled type should be classified");
        investimento.RemoverClassificacaoTipoAtivo();
        investimento.DesassociarGrupoObjetivo();
        TestAssert.True(investimento.TipoAtivo == null && investimento.GrupoObjetivoInvestimentoId == null, "Organization should be clearable");
        return Task.CompletedTask;
    }
}
