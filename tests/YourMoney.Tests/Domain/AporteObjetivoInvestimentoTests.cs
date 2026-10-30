using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain;
public static class AporteObjetivoInvestimentoTests
{
    public static async Task ValidatesPrecisionAndImmutableAllocation()
    {
        await TestAssert.ThrowsAsync<ArgumentException>(() => Task.FromResult(new AporteObjetivoInvestimento("u", Guid.NewGuid(), Guid.NewGuid(), 1.001m, DateTime.Today, DateTime.Today, Guid.NewGuid())), "More than two cents must fail");
        var aporte = new AporteObjetivoInvestimento("u", Guid.NewGuid(), Guid.NewGuid(), .01m, DateTime.Today, DateTime.Today, Guid.NewGuid());
        var destino = Guid.NewGuid(); aporte.Alocar(destino); aporte.Alocar(destino);
        await TestAssert.ThrowsAsync<InvalidOperationException>(() => { aporte.Alocar(Guid.NewGuid()); return Task.CompletedTask; }, "Destination cannot be replaced");
    }
}
