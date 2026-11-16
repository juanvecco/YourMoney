using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain;

public static class DespesaConferenciaTests
{
    public static Task ChangesAndResetsOnEdit()
    {
        var despesa = new Despesa("Compra", 12.34m, new DateTime(2026, 11, 1),
            DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user");
        TestAssert.True(!despesa.Conferida, "New expense must be pending");
        despesa.DefinirConferida(true);
        TestAssert.True(despesa.Conferida, "Explicit review must be saved");
        despesa.AtualizarValor(12.35m);
        TestAssert.True(!despesa.Conferida, "Value edit must reset review");
        return Task.CompletedTask;
    }
}
