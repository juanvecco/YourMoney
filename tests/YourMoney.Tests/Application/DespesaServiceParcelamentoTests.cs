using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class DespesaServiceParcelamentoTests
    {
        public static async Task CentDistributionPreservesExactTotal()
        {
            var service = DespesaTestFixtures.CreateService();
            var response = await service.CriarParcelamentoAsync(DespesaTestFixtures.Request(100m, 3));

            TestAssert.Equal(3, response.Parcelas.Count, "Should create three installments");
            TestAssert.Equal(100m, response.Parcelas.Sum(p => p.Valor), "Installments should preserve exact total");
            TestAssert.Equal(33.34m, response.Parcelas[0].Valor, "First installment should receive first remainder cent");
            TestAssert.Equal(33.33m, response.Parcelas[1].Valor, "Second installment should use base value");
            TestAssert.Equal(33.33m, response.Parcelas[2].Valor, "Third installment should use base value");
        }

        public static async Task EndOfMonthDatesClampPredictably()
        {
            var service = DespesaTestFixtures.CreateService();
            var response = await service.CriarParcelamentoAsync(DespesaTestFixtures.Request(90m, 3, new DateTime(2026, 1, 31)));

            TestAssert.Equal(new DateTime(2026, 1, 31), response.Parcelas[0].Data, "First installment date mismatch");
            TestAssert.Equal(new DateTime(2026, 2, 28), response.Parcelas[1].Data, "February installment should clamp to last day");
            TestAssert.Equal(new DateTime(2026, 3, 31), response.Parcelas[2].Data, "March installment should preserve original day");
        }

        public static async Task AtomicCreationSharesGroupId()
        {
            var repository = new InMemoryDespesaRepository();
            var service = DespesaTestFixtures.CreateService(repository);
            var response = await service.CriarParcelamentoAsync(DespesaTestFixtures.Request(120m, 4));

            TestAssert.Equal(4, repository.Despesas.Count, "Repository should receive all installments");
            TestAssert.True(response.ParcelamentoId.HasValue, "Installment group id should be populated");
            TestAssert.True(repository.Despesas.All(d => d.ParcelamentoId == response.ParcelamentoId), "All installments should share group id");
        }

        public static async Task MonthlyQueryIncludesInstallmentMetadata()
        {
            var repository = new InMemoryDespesaRepository();
            var service = DespesaTestFixtures.CreateService(repository);
            var created = await service.CriarParcelamentoAsync(DespesaTestFixtures.Request(90m, 3, new DateTime(2026, 1, 31)));

            var february = await service.ObterPorMesAnoAsync(2, 2026);
            var parcela = february.Single();

            TestAssert.Equal(created.ParcelamentoId, parcela.ParcelamentoId, "Query should include installment group id");
            TestAssert.Equal(2, parcela.NumeroParcela, "Query should include installment number");
            TestAssert.Equal(3, parcela.TotalParcelas, "Query should include total installments");
            TestAssert.Equal(90m, parcela.ValorTotalParcelamento, "Query should include original total");
        }

        public static async Task MonthlyQueryKeepsRegularMetadataNull()
        {
            var repository = new InMemoryDespesaRepository();
            await repository.AdicionarAsync(new Despesa("Mercado", 200m, new DateTime(2026, 6, 10), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user"));
            var service = DespesaTestFixtures.CreateService(repository);

            var june = await service.ObterPorMesAnoAsync(6, 2026);
            var despesa = june.Single();

            TestAssert.Equal<Guid?>(null, despesa.ParcelamentoId, "Regular expense should not have group id");
            TestAssert.Equal<int?>(null, despesa.NumeroParcela, "Regular expense should not have installment number");
            TestAssert.Equal<int?>(null, despesa.TotalParcelas, "Regular expense should not have total installments");
            TestAssert.Equal<decimal?>(null, despesa.ValorTotalParcelamento, "Regular expense should not have original total");
        }

        public static async Task InvalidInstallmentQuantityFails()
        {
            var service = DespesaTestFixtures.CreateService();

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarParcelamentoAsync(DespesaTestFixtures.Request(100m, 0)),
                "Quantity below 1 should fail");

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarParcelamentoAsync(DespesaTestFixtures.Request(100m, 121)),
                "Quantity above 120 should fail");
        }

        public static async Task InvalidValueAndDescriptionFail()
        {
            var service = DespesaTestFixtures.CreateService();
            var blank = DespesaTestFixtures.Request();
            blank.Descricao = " ";

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarParcelamentoAsync(DespesaTestFixtures.Request(0m, 3)),
                "Zero total should fail");

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarParcelamentoAsync(blank),
                "Blank description should fail");
        }

        public static async Task MissingAccountOrCategoryFails()
        {
            var missingContaService = DespesaTestFixtures.CreateService(contaExists: false);
            var missingCategoriaService = DespesaTestFixtures.CreateService(categoriaExists: false);

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => missingContaService.CriarParcelamentoAsync(DespesaTestFixtures.Request()),
                "Missing account should fail");

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => missingCategoriaService.CriarParcelamentoAsync(DespesaTestFixtures.Request()),
                "Missing category should fail");
        }

        public static async Task BatchFailureLeavesNoPartialRows()
        {
            var repository = new InMemoryDespesaRepository { FailBatchInsert = true };
            var service = DespesaTestFixtures.CreateService(repository);

            await TestAssert.ThrowsAsync<InvalidOperationException>(
                () => service.CriarParcelamentoAsync(DespesaTestFixtures.Request()),
                "Batch persistence failure should surface");

            TestAssert.Equal(0, repository.Despesas.Count, "Failed batch should not leave partial rows");
        }
    }
}
