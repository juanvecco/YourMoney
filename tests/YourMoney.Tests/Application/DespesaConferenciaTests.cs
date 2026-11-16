using YourMoney.Tests.Fixtures;
using YourMoney.Domain.Entities;

namespace YourMoney.Tests.Application;

public static class DespesaConferenciaTests
{
    public static async Task UsesAuthenticatedOwner()
    {
        var repo = new InMemoryDespesaRepository();
        var foreign = new Despesa("Foreign", 2m, new DateTime(2026, 11, 1),
            DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "other-user");
        repo.Despesas.Add(foreign);
        var service = DespesaTestFixtures.CreateService(despesaRepository: repo);
        await TestAssert.ThrowsAsync<InvalidOperationException>(
            () => service.DefinirConferenciaAsync(foreign.Id, true),
            "Foreign expense must not be changed");
        TestAssert.True(!foreign.Conferida, "Foreign expense remains pending");
    }
}
