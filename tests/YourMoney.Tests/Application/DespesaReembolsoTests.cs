using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class DespesaReembolsoTests
    {
        public static async Task ReimbursementReducesLiquidExpense()
        {
            var receitaRepository = new InMemoryReceitaRepository();
            var despesaRepository = new InMemoryDespesaRepository();
            var despesa = new Despesa("Compra para terceiro", 150m, new DateTime(2026, 7, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user");
            despesaRepository.Despesas.Add(despesa);
            var receitaService = ReceitaTestFixtures.CreateService(receitaRepository, despesaRepository: despesaRepository);
            var despesaService = DespesaTestFixtures.CreateService(despesaRepository, receitaRepository);

            await receitaService.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest(
                descricao: "Reembolso compra para terceiro",
                valor: 150m,
                data: new DateTime(2026, 7, 5),
                mesReferencia: new DateTime(2026, 7, 1),
                natureza: "Reembolso",
                despesaVinculadaId: despesa.Id));

            var despesas = await despesaService.ObterPorMesAnoAsync(7, 2026);
            var dto = despesas.Single();

            TestAssert.Equal(150m, dto.Valor, "Gross expense should be preserved");
            TestAssert.Equal(150m, dto.ValorReembolsado, "Reimbursed value should be exposed");
            TestAssert.Equal(0m, dto.ValorLiquido, "Liquid expense should subtract reimbursement");
            TestAssert.True(dto.PossuiReembolso, "Expense should indicate reimbursement");
        }

        public static async Task RejectsReimbursementAbovePendingExpense()
        {
            var receitaRepository = new InMemoryReceitaRepository();
            var despesaRepository = new InMemoryDespesaRepository();
            var despesa = new Despesa("Compra para terceiro", 100m, new DateTime(2026, 7, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user");
            despesaRepository.Despesas.Add(despesa);
            var receitaService = ReceitaTestFixtures.CreateService(receitaRepository, despesaRepository: despesaRepository);

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => receitaService.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest(
                    descricao: "Reembolso acima",
                    valor: 150m,
                    data: new DateTime(2026, 7, 5),
                    mesReferencia: new DateTime(2026, 7, 1),
                    natureza: "Reembolso",
                    despesaVinculadaId: despesa.Id)),
                "Reimbursement above pending expense should be rejected");
        }

        public static async Task RejectsReimbursementForDifferentUserExpense()
        {
            var receitaRepository = new InMemoryReceitaRepository();
            var despesaRepository = new InMemoryDespesaRepository();
            var despesa = new Despesa("Compra de outro usuário", 100m, new DateTime(2026, 7, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "user-b");
            despesaRepository.Despesas.Add(despesa);
            var receitaService = ReceitaTestFixtures.CreateService(receitaRepository, despesaRepository: despesaRepository);

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => receitaService.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest(
                    descricao: "Reembolso indevido",
                    valor: 100m,
                    data: new DateTime(2026, 7, 5),
                    mesReferencia: new DateTime(2026, 7, 1),
                    natureza: "Reembolso",
                    despesaVinculadaId: despesa.Id)),
                "Reimbursement should not link another user's expense");
        }
    }
}
