using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain
{
    public static class ReceitaNaturezaTests
    {
        public static Task DefaultsToAvailableIncome()
        {
            var receita = new Receita("Salário", 5000m, new DateTime(2026, 7, 5), new DateTime(2026, 7, 1), "test-user");

            TestAssert.Equal(NaturezaReceita.RendaDisponivel, receita.Natureza, "Receita should default to available income");
            TestAssert.True(receita.ConsideraNasMetas, "Available income should count for goals");
            TestAssert.Equal<Guid?>(null, receita.DespesaVinculadaId, "Available income should not link expense");

            return Task.CompletedTask;
        }

        public static async Task ValidatesReimbursementLink()
        {
            var receita = new Receita("Reembolso", 150m, new DateTime(2026, 7, 5), new DateTime(2026, 7, 1), "test-user");

            await TestAssert.ThrowsAsync<ArgumentException>(
                () =>
                {
                    receita.AtualizarNatureza(NaturezaReceita.Reembolso);
                    return Task.CompletedTask;
                },
                "Reimbursement should require linked expense");

            var despesaId = Guid.NewGuid();
            receita.AtualizarNatureza(NaturezaReceita.Reembolso, despesaId);

            TestAssert.Equal(NaturezaReceita.Reembolso, receita.Natureza, "Nature should change to reimbursement");
            TestAssert.Equal<Guid?>(despesaId, receita.DespesaVinculadaId, "Reimbursement should keep linked expense");
            TestAssert.True(!receita.ConsideraNasMetas, "Reimbursement should not count for goals");
        }

        public static Task ClearsExpenseLinkWhenNotReimbursement()
        {
            var despesaId = Guid.NewGuid();
            var receita = new Receita("Reembolso", 150m, new DateTime(2026, 7, 5), new DateTime(2026, 7, 1), "test-user", NaturezaReceita.Reembolso, despesaId);

            receita.AtualizarNatureza(NaturezaReceita.EntradaVinculadaDespesa);

            TestAssert.Equal(NaturezaReceita.EntradaVinculadaDespesa, receita.Natureza, "Nature should change");
            TestAssert.Equal<Guid?>(null, receita.DespesaVinculadaId, "Non-reimbursement should clear linked expense");
            return Task.CompletedTask;
        }
    }
}
