using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
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

        public static Task SupportsValueAndModeTransitions()
        {
            var meta = new MetaMensal(
                "Reserva",
                TipoDefinicaoMeta.Valor,
                null,
                1000.25m,
                new DateTime(2026, 6, 20),
                "user-1");

            TestAssert.Equal(TipoDefinicaoMeta.Valor, meta.TipoDefinicao, "Value mode should be preserved");
            TestAssert.Equal(1000.25m, meta.ValorMeta, "Primary monetary value should be preserved");
            TestAssert.Equal<decimal?>(null, meta.PercentualReceita, "Inactive percentage should stay null");

            meta.Atualizar("Reserva percentual", TipoDefinicaoMeta.Percentual, 20.1234m, null);

            TestAssert.Equal(TipoDefinicaoMeta.Percentual, meta.TipoDefinicao, "Mode should change to percentage");
            TestAssert.Equal(20.1234m, meta.PercentualReceita, "Primary percentage should be preserved");
            TestAssert.Equal<decimal?>(null, meta.ValorMeta, "Previous monetary value should be cleared");
            TestAssert.Equal(new DateTime(2026, 6, 1), meta.MesReferencia, "Changing mode must preserve reference month");
            return Task.CompletedTask;
        }

        public static async Task RejectsInconsistentDefinitionsAndPrecision()
        {
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(new MetaMensal(
                    "Meta",
                    TipoDefinicaoMeta.Valor,
                    20m,
                    1000m,
                    new DateTime(2026, 6, 1))),
                "Both principal fields should be rejected");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(new MetaMensal(
                    "Meta",
                    TipoDefinicaoMeta.Valor,
                    null,
                    10.001m,
                    new DateTime(2026, 6, 1))),
                "Monetary values beyond cents should be rejected");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(new MetaMensal(
                    "Meta",
                    TipoDefinicaoMeta.Percentual,
                    10.12345m,
                    null,
                    new DateTime(2026, 6, 1))),
                "Percentages beyond four decimals should be rejected");
        }
    }
}
