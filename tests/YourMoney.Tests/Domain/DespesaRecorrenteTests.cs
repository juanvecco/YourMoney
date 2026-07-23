using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain
{
    public static class DespesaRecorrenteTests
    {
        public static Task CreatesMonthlyRecurrenceAndClampsDueDate()
        {
            var recorrencia = CriarRecorrencia(
                dataVencimento: new DateTime(2026, 1, 31),
                dataInicio: new DateTime(2026, 1, 5),
                dataTermino: new DateTime(2026, 3, 20));

            TestAssert.Equal(31, recorrencia.DiaVencimento, "Recurrence should store due day from due date");
            TestAssert.Equal(120.36m, recorrencia.ValorPrevisto, "Recurrence should round expected value");
            TestAssert.True(recorrencia.EstaElegivelParaMes(new DateTime(2026, 2, 10)), "Recurrence should be eligible inside configured period");
            TestAssert.True(!recorrencia.EstaElegivelParaMes(new DateTime(2026, 4, 1)), "Recurrence should not be eligible after end date month");
            TestAssert.Equal(new DateTime(2026, 2, 28), recorrencia.CalcularDataSugerida(new DateTime(2026, 2, 1)), "Suggested date should clamp to last day of shorter month");

            return Task.CompletedTask;
        }

        public static Task OccurrenceCanBeConfirmedOrIgnoredOnlyOnce()
        {
            var ocorrencia = new DespesaRecorrenteOcorrencia(Guid.NewGuid(), new DateTime(2026, 5, 20), "test-user");
            var despesaConfirmadaId = Guid.NewGuid();

            ocorrencia.Confirmar(despesaConfirmadaId);

            TestAssert.Equal(StatusDespesaRecorrenteOcorrencia.Confirmada, ocorrencia.Status, "Confirmed occurrence should change status");
            TestAssert.Equal(despesaConfirmadaId, ocorrencia.DespesaConfirmadaId!.Value, "Confirmed occurrence should keep generated expense id");
            TestAssert.True(!ocorrencia.EstaPendente, "Confirmed occurrence should no longer be pending");

            return TestAssert.ThrowsAsync<InvalidOperationException>(
                () => Task.Run(() => ocorrencia.Ignorar()),
                "Finalized occurrence should not be ignored later");
        }

        public static Task RejectsInvalidRecurrencePeriod()
        {
            return TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.Run(() => CriarRecorrencia(
                    dataInicio: new DateTime(2026, 5, 1),
                    dataTermino: new DateTime(2026, 4, 30))),
                "Recurrence should reject end date before start date");
        }

        private static DespesaRecorrente CriarRecorrencia(
            DateTime? dataVencimento = null,
            DateTime? dataInicio = null,
            DateTime? dataTermino = null)
        {
            return new DespesaRecorrente(
                "Internet",
                120.356m,
                DespesaTestFixtures.ContaId,
                DespesaTestFixtures.TipoEssencialId,
                DespesaTestFixtures.NaturezaMoradiaId,
                DespesaTestFixtures.CategoriaAluguelId,
                dataVencimento ?? new DateTime(2026, 5, 10),
                dataInicio ?? new DateTime(2026, 5, 1),
                dataTermino,
                "test-user");
        }
    }
}
