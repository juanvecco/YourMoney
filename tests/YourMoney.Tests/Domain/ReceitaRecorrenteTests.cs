using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain
{
    public static class ReceitaRecorrenteTests
    {
        public static Task CreatesValidRecurrenceAndClampsReceivingDate()
        {
            var recorrencia = Criar(dataRecebimento: new DateTime(2026, 1, 31));

            TestAssert.Equal(5000.36m, recorrencia.ValorPrevisto, "Recurring value should round to cents");
            TestAssert.Equal(31, recorrencia.DiaRecebimento, "Receiving day should come from the selected date");
            TestAssert.Equal(new DateTime(2026, 2, 28), recorrencia.CalcularDataSugerida(new DateTime(2026, 2, 10)), "Short months should clamp the receiving date");
            TestAssert.True(recorrencia.EhSalario, "Salary flag should be preserved");
            TestAssert.True(recorrencia.ConsideraReservaEmergencia, "Reserve flag should be preserved independently");
            return Task.CompletedTask;
        }

        public static async Task RejectsInvalidDataAndReimbursementNature()
        {
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.Run(() => Criar(descricao: " ")),
                "Description should be required");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.Run(() => Criar(valor: 0)),
                "Value should be positive");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.Run(() => Criar(natureza: NaturezaReceita.Reembolso)),
                "Reimbursement should not be recurring");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.Run(() => Criar(dataInicio: new DateTime(2026, 8, 1), dataTermino: new DateTime(2026, 7, 31))),
                "End should not precede start");
        }

        public static Task OccurrenceFinalizesOnlyOnce()
        {
            var ocorrencia = new ReceitaRecorrenteOcorrencia(Guid.NewGuid(), new DateTime(2026, 7, 18), "test-user");
            ocorrencia.Ignorar();

            TestAssert.Equal(StatusReceitaRecorrenteOcorrencia.Ignorada, ocorrencia.Status, "Ignore should finalize the monthly occurrence");
            TestAssert.True(!ocorrencia.EstaPendente, "Ignored occurrence should no longer be pending");
            return TestAssert.ThrowsAsync<InvalidOperationException>(
                () => Task.Run(() => ocorrencia.Confirmar(Guid.NewGuid())),
                "A finalized occurrence should not be confirmed later");
        }

        public static Task MaintenancePreservesConfiguredPeriod()
        {
            var recorrencia = Criar();
            recorrencia.Encerrar(new DateTime(2026, 8, 31));
            recorrencia.Desativar();

            TestAssert.Equal(new DateTime(2026, 8, 31), recorrencia.DataTermino!.Value, "End should preserve civil date");
            TestAssert.True(!recorrencia.Ativa, "Deactivate should make future months ineligible");
            TestAssert.True(!recorrencia.EstaElegivelParaMes(new DateTime(2026, 7, 1)), "Inactive recurrence should not be eligible");
            return Task.CompletedTask;
        }

        private static ReceitaRecorrente Criar(
            string descricao = "Salário",
            decimal valor = 5000.356m,
            NaturezaReceita natureza = NaturezaReceita.RendaDisponivel,
            DateTime? dataRecebimento = null,
            DateTime? dataInicio = null,
            DateTime? dataTermino = null)
        {
            return new ReceitaRecorrente(
                descricao,
                valor,
                DespesaTestFixtures.ContaId,
                natureza,
                true,
                true,
                dataRecebimento ?? new DateTime(2026, 1, 5),
                dataInicio ?? new DateTime(2026, 1, 1),
                dataTermino,
                "test-user");
        }
    }
}
