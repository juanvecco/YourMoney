using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain
{
    public static class MetaMensalTests
    {
        public static Task CreatesNormalizedOwnedMeta()
        {
            var meta = new MetaMensal(" Investimento ", 25.5m, new DateTime(2026, 6, 27), "user-1");

            TestAssert.True(meta.Id != Guid.Empty, "Meta should receive an id");
            TestAssert.Equal("Investimento", meta.Nome, "Name should be trimmed");
            TestAssert.Equal(25.5m, meta.PercentualReceita, "Percentage should be preserved");
            TestAssert.Equal(new DateTime(2026, 6, 1), meta.MesReferencia, "Reference month should normalize to first day");
            TestAssert.Equal("user-1", meta.UsuarioId, "Meta should keep owner");
            return Task.CompletedTask;
        }

        public static async Task RejectsInvalidData()
        {
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(new MetaMensal(" ", 25m, new DateTime(2026, 6, 1))),
                "Blank name should be rejected");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(new MetaMensal("Meta", 0m, new DateTime(2026, 6, 1))),
                "Zero percentage should be rejected");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(new MetaMensal("Meta", 1m, default)),
                "Default month should be rejected");
        }
    }
}
