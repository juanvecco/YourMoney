using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class MetaMensalServiceTests
    {
        public static async Task CreatesEditsDeletesAndCalculatesMeta()
        {
            var metaRepository = new InMemoryMetaMensalRepository();
            var receitaRepository = new InMemoryReceitaRepository();
            var despesaRepository = new InMemoryDespesaRepository();
            var service = MetaMensalTestFixtures.CreateService(metaRepository, receitaRepository, despesaRepository);
            var receita = new Receita("Salário", 10000m, new DateTime(2026, 6, 5), new DateTime(2026, 6, 1), "test-user");
            receitaRepository.Receitas.Add(receita);

            var created = await service.CriarAsync(MetaMensalTestFixtures.CriarRequest());

            TestAssert.Equal(1, metaRepository.Metas.Count, "Create should persist one meta");
            TestAssert.Equal(2500m, created.ValorCalculado, "25 percent of revenue should be calculated");

            var updated = await service.AtualizarAsync(created.Id, new()
            {
                Id = created.Id,
                Nome = "Dízimo",
                PercentualReceita = 10m
            });

            TestAssert.Equal("Dízimo", updated.Nome, "Update should change name");
            TestAssert.Equal(1000m, updated.ValorCalculado, "10 percent of revenue should be calculated");

            await service.RemoverAsync(created.Id);

            TestAssert.Equal(0, metaRepository.Metas.Count, "Delete should remove the meta");
            TestAssert.Equal(1, receitaRepository.Receitas.Count, "Goal operations must not mutate receitas");
            TestAssert.Equal(0, despesaRepository.Despesas.Count, "Goal operations must not mutate despesas");
        }

        public static async Task CalculatesMonthlySummary()
        {
            var metaRepository = new InMemoryMetaMensalRepository();
            var receitaRepository = new InMemoryReceitaRepository();
            var despesaRepository = new InMemoryDespesaRepository();
            var service = MetaMensalTestFixtures.CreateService(metaRepository, receitaRepository, despesaRepository);
            receitaRepository.Receitas.Add(new Receita("Salário", 10000m, new DateTime(2026, 6, 5), new DateTime(2026, 6, 1), "test-user"));
            despesaRepository.Despesas.Add(new Despesa("Mercado", 2500m, new DateTime(2026, 6, 10), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user"));
            await service.CriarAsync(MetaMensalTestFixtures.CriarRequest("Investimento", 25m, new DateTime(2026, 6, 1)));
            await service.CriarAsync(MetaMensalTestFixtures.CriarRequest("Dízimo", 10m, new DateTime(2026, 6, 1)));

            var resumo = await service.ObterResumoAsync(6, 2026);

            TestAssert.Equal(10000m, resumo.ReceitaTotal, "Revenue total should match month");
            TestAssert.Equal(2500m, resumo.DespesaTotal, "Expense total should match month");
            TestAssert.Equal(35m, resumo.PercentualTotalComprometido, "Committed percentage should sum goals");
            TestAssert.Equal(3500m, resumo.ValorTotalReservado, "Reserved amount should sum goals");
            TestAssert.Equal(65m, resumo.PercentualRestante, "Remaining percentage should subtract goals");
            TestAssert.Equal(6500m, resumo.ValorRestanteAntesDespesas, "Remaining before expenses should subtract goals");
            TestAssert.Equal(4000m, resumo.SaldoFinal, "Final balance should subtract goals and expenses");
            TestAssert.Equal("disponivel", resumo.Status, "Positive balance should be available");
        }

        public static async Task CalculatesGoalsFromEligibleRevenue()
        {
            var metaRepository = new InMemoryMetaMensalRepository();
            var receitaRepository = new InMemoryReceitaRepository();
            var despesaRepository = new InMemoryDespesaRepository();
            var service = MetaMensalTestFixtures.CreateService(metaRepository, receitaRepository, despesaRepository);
            receitaRepository.Receitas.Add(new Receita("Salário", 5000m, new DateTime(2026, 7, 5), new DateTime(2026, 7, 1), "test-user"));
            receitaRepository.Receitas.Add(new Receita("Vale alimentação", 800m, new DateTime(2026, 7, 5), new DateTime(2026, 7, 1), "test-user", NaturezaReceita.EntradaVinculadaDespesa));
            var despesa = new Despesa("Compra para terceiro", 150m, new DateTime(2026, 7, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user");
            despesaRepository.Despesas.Add(despesa);
            receitaRepository.Receitas.Add(new Receita("Reembolso", 150m, new DateTime(2026, 7, 5), new DateTime(2026, 7, 1), "test-user", NaturezaReceita.Reembolso, despesa.Id));

            var meta = await service.CriarAsync(MetaMensalTestFixtures.CriarRequest("Reserva", 20m, new DateTime(2026, 7, 1)));
            var resumo = await service.ObterResumoAsync(7, 2026);

            TestAssert.Equal(1000m, meta.ValorCalculado, "Goal value should use only eligible revenue");
            TestAssert.Equal(5000m, resumo.ReceitaTotal, "Legacy revenue total should expose eligible revenue");
            TestAssert.Equal(5950m, resumo.ReceitaTotalBruta, "Gross revenue should include restricted income and reimbursements");
            TestAssert.Equal(5000m, resumo.ReceitaElegivelMetas, "Eligible revenue should include only available income");
            TestAssert.Equal(950m, resumo.ReceitaExcluidaMetas, "Excluded revenue should be gross minus eligible revenue");
            TestAssert.Equal(150m, resumo.DespesaTotalBruta, "Gross expenses should be preserved");
            TestAssert.Equal(150m, resumo.DespesaTotalReembolsada, "Reimbursed expense total should be exposed");
            TestAssert.Equal(0m, resumo.DespesaTotal, "Liquid expenses should subtract reimbursements");
            TestAssert.Equal(4000m, resumo.SaldoFinal, "Final balance should use eligible revenue and liquid expenses");
        }

        public static async Task ShowsAlertsForExceededPlanningAndZeroRevenue()
        {
            var metaRepository = new InMemoryMetaMensalRepository();
            var receitaRepository = new InMemoryReceitaRepository();
            var despesaRepository = new InMemoryDespesaRepository();
            var service = MetaMensalTestFixtures.CreateService(metaRepository, receitaRepository, despesaRepository);
            await service.CriarAsync(MetaMensalTestFixtures.CriarRequest("Investimento", 110m, new DateTime(2026, 6, 1)));

            var zeroRevenue = await service.ObterResumoAsync(6, 2026);

            TestAssert.Equal(0m, zeroRevenue.Metas.Single().ValorCalculado, "Zero revenue should calculate zero goal values");
            TestAssert.True(zeroRevenue.Alertas.Any(a => a.Contains("não há receita")), "Zero revenue should be called out");
            TestAssert.True(zeroRevenue.Alertas.Any(a => a.Contains("100%")), "Over 100 percent should warn");

            receitaRepository.Receitas.Add(new Receita("Salário", 10000m, new DateTime(2026, 6, 5), new DateTime(2026, 6, 1), "test-user"));
            var exceeded = await service.ObterResumoAsync(6, 2026);

            TestAssert.Equal(-10m, exceeded.PercentualRestante, "Over 100 percent should produce negative remaining percent");
            TestAssert.Equal(-1000m, exceeded.SaldoFinal, "Over 100 percent should produce negative final balance");
            TestAssert.Equal(1000m, exceeded.ValorFaltante, "Missing amount should be absolute negative balance");
            TestAssert.Equal("faltando", exceeded.Status, "Negative balance should be missing status");
        }
    }
}
