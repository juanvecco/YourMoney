using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;
public static class AporteObjetivoInvestimentoServiceTests
{
    public static Task OperationIdentityDetectsReplayAndDivergence()
    {
        var meta = Guid.NewGuid(); var grupo = Guid.NewGuid(); var operacao = Guid.NewGuid(); var mes = new DateTime(2026, 10, 1); var data = new DateTime(2026, 10, 26);
        var aporte = new AporteObjetivoInvestimento("owner", meta, grupo, 600m, mes, data, operacao);
        TestAssert.True(aporte.Corresponde(meta, grupo, 600m, mes, data, null), "Identical replay must match");
        TestAssert.True(!aporte.Corresponde(meta, grupo, 601m, mes, data, null), "Divergent replay must conflict");
        return Task.CompletedTask;
    }
}
