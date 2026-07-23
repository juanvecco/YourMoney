using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain
{
    public static class InvestimentoReservaTests
    {
        public static Task CreatesOptionalSalaryLinkAndOperation()
        {
            var recorrenciaId = Guid.NewGuid();
            var operacaoId = Guid.NewGuid();
            var investimento = new Investimento(
                "Tesouro Selic",
                "Reserva de emergência",
                "Renda fixa",
                1m,
                100m,
                100m,
                new DateTime(2026, 7, 22),
                new DateTime(2026, 7, 1),
                "user-1",
                recorrenciaId,
                operacaoId);

            TestAssert.Equal(recorrenciaId, investimento.ReceitaRecorrenteId, "Investment should preserve optional recurring salary link");
            TestAssert.Equal(operacaoId, investimento.OperacaoId, "Investment should preserve idempotency operation");
            return Task.CompletedTask;
        }

        public static Task ChangesAndRemovesSalaryLink()
        {
            var investimento = new Investimento(
                "Tesouro Selic",
                "Reserva de emergência",
                "Renda fixa",
                1m,
                100m,
                100m,
                new DateTime(2026, 7, 22),
                new DateTime(2026, 7, 1),
                "user-1");
            var recorrenciaId = Guid.NewGuid();

            investimento.DefinirReceitaRecorrente(recorrenciaId);
            TestAssert.Equal(recorrenciaId, investimento.ReceitaRecorrenteId, "Investment should accept a salary link");

            investimento.DefinirReceitaRecorrente(null);
            TestAssert.True(investimento.ReceitaRecorrenteId == null, "Investment should allow removing a salary link");
            return Task.CompletedTask;
        }

        public static async Task RequiresDescriptionForNewWrites()
        {
            await TestAssert.ThrowsAsync<ArgumentException>(() => Task.FromResult(new Investimento(
                "Tesouro", " ", "Renda fixa", 1m, 100m, 100m,
                new DateTime(2026, 7, 22), new DateTime(2026, 7, 1), "user-1")),
                "Investment should require a description");
        }
    }
}
