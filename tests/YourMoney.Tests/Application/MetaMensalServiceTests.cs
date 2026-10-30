using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Application.Services;
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
                TipoDefinicao = "Percentual",
                PercentualReceita = 10m,
                ValorMeta = null
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

        public static async Task CreatesBothModesAndRecalculatesDerivedFields()
        {
            var metas = new InMemoryMetaMensalRepository();
            var receitas = new InMemoryReceitaRepository();
            var service = MetaMensalTestFixtures.CreateService(metas, receitas);
            receitas.Receitas.Add(new Receita(
                "Salário",
                5000m,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 1),
                "test-user"));

            var porValor = await service.CriarAsync(
                MetaMensalTestFixtures.CriarRequestPorValor("Reserva", 1000m));
            var porPercentual = await service.CriarAsync(
                MetaMensalTestFixtures.CriarRequest("Investimento", 20m));

            TestAssert.Equal("Valor", porValor.TipoDefinicao, "Value mode should be returned");
            TestAssert.Equal(1000m, porValor.ValorMeta, "Value input should remain the principal field");
            TestAssert.Equal(20m, porValor.PercentualReceita, "Percentage should be derived from eligible revenue");
            TestAssert.Equal("Percentual", porPercentual.TipoDefinicao, "Percentage mode should be returned");
            TestAssert.Equal<decimal?>(null, porPercentual.ValorMeta, "Derived amount must not be persisted as principal value");
            TestAssert.Equal(1000m, porPercentual.ValorCalculado, "Amount should be derived from percentage");

            var resumo = await service.ObterResumoAsync(6, 2026);
            TestAssert.Equal(40m, resumo.PercentualTotalComprometido, "Mixed effective percentages should sum");
            TestAssert.Equal(2000m, resumo.ValorTotalReservado, "Mixed effective amounts should sum");

            receitas.Receitas.Clear();
            receitas.Receitas.Add(new Receita(
                "Salário",
                10000m,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 1),
                "test-user"));

            var recalculado = await service.ObterResumoAsync(6, 2026);
            var valorFixo = recalculado.Metas.Single(meta => meta.TipoDefinicao == "Valor");
            var percentualFixo = recalculado.Metas.Single(meta => meta.TipoDefinicao == "Percentual");
            TestAssert.Equal(1000m, valorFixo.ValorCalculado, "Value goal should keep its principal amount");
            TestAssert.Equal(10m, valorFixo.PercentualReceita, "Value goal percentage should be recalculated");
            TestAssert.Equal(20m, percentualFixo.PercentualReceita, "Percentage goal should keep its principal percentage");
            TestAssert.Equal(2000m, percentualFixo.ValorCalculado, "Percentage goal amount should be recalculated");
        }

        public static async Task EditsModesAndPreservesReferenceMonth()
        {
            var metas = new InMemoryMetaMensalRepository();
            var receitas = new InMemoryReceitaRepository();
            var service = MetaMensalTestFixtures.CreateService(metas, receitas);
            receitas.Receitas.Add(new Receita(
                "Salário",
                5000m,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 1),
                "test-user"));
            var criada = await service.CriarAsync(MetaMensalTestFixtures.CriarRequestPorValor());

            var atualizada = await service.AtualizarAsync(criada.Id, new()
            {
                Id = criada.Id,
                Nome = "Reserva maior",
                TipoDefinicao = "Valor",
                ValorMeta = 1500m,
                PercentualReceita = null
            });
            TestAssert.Equal(30m, atualizada.PercentualReceita, "Edited value should derive 30 percent");

            var trocada = await service.AtualizarAsync(criada.Id, new()
            {
                Id = criada.Id,
                Nome = "Reserva proporcional",
                TipoDefinicao = "Percentual",
                PercentualReceita = 25m,
                ValorMeta = null
            });
            TestAssert.Equal("Percentual", trocada.TipoDefinicao, "Update should allow changing mode");
            TestAssert.Equal(1250m, trocada.ValorCalculado, "Changed percentage should derive amount");
            TestAssert.Equal(new DateTime(2026, 6, 1), trocada.MesReferencia, "Update should preserve original month");
            TestAssert.Equal<decimal?>(null, metas.Metas.Single().ValorMeta, "Old principal value should be cleared");
        }

        public static async Task HandlesZeroBaseValueGoalsAndSafeRename()
        {
            var metas = new InMemoryMetaMensalRepository();
            var service = MetaMensalTestFixtures.CreateService(metas);

            await TestAssert.ThrowsAsync<ConflitoMetaMensalException>(
                () => service.CriarAsync(MetaMensalTestFixtures.CriarRequestPorValor()),
                "Creating a value goal without positive eligible revenue should conflict");

            var historica = new MetaMensal(
                "Reserva histórica",
                TipoDefinicaoMeta.Valor,
                null,
                1000m,
                new DateTime(2026, 6, 1),
                "test-user");
            metas.Metas.Add(historica);

            var resumo = await service.ObterResumoAsync(6, 2026);
            TestAssert.Equal<decimal?>(null, resumo.Metas.Single().PercentualReceita, "Derived percentage should be unavailable");
            TestAssert.Equal<decimal?>(null, resumo.PercentualTotalComprometido, "Mixed percentage total should be unavailable");
            TestAssert.Equal(1000m, resumo.ValorTotalReservado, "Fixed amount should still count in total");
            TestAssert.True(resumo.Alertas.Any(alerta => alerta.Contains("receita elegível positiva")), "Summary should explain unavailable percentages");

            var renomeada = await service.AtualizarAsync(historica.Id, new()
            {
                Id = historica.Id,
                Nome = "Reserva renomeada",
                TipoDefinicao = "Valor",
                PercentualReceita = null,
                ValorMeta = 1000m
            });
            TestAssert.Equal("Reserva renomeada", renomeada.Nome, "Same value definition may be renamed without revenue");

            await TestAssert.ThrowsAsync<ConflitoMetaMensalException>(
                () => service.AtualizarAsync(historica.Id, new()
                {
                    Id = historica.Id,
                    Nome = "Reserva alterada",
                    TipoDefinicao = "Valor",
                    PercentualReceita = null,
                    ValorMeta = 1500m
                }),
                "Changing a value goal without revenue should conflict");
        }

        public static async Task RejectsInvalidDiscriminatedPayloadsAndForeignGoals()
        {
            var metas = new InMemoryMetaMensalRepository();
            var receitas = new InMemoryReceitaRepository();
            var service = MetaMensalTestFixtures.CreateService(metas, receitas);
            receitas.Receitas.Add(new Receita(
                "Salário",
                5000m,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 1),
                "test-user"));

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarAsync(new()
                {
                    Nome = "Inválida",
                    TipoDefinicao = "Valor",
                    PercentualReceita = 20m,
                    ValorMeta = 1000m,
                    MesReferencia = new DateTime(2026, 6, 1)
                }),
                "Payload with both fields should be rejected");
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarAsync(new()
                {
                    Nome = "Inválida",
                    TipoDefinicao = "Outra",
                    ValorMeta = 1000m,
                    MesReferencia = new DateTime(2026, 6, 1)
                }),
                "Unknown mode should be rejected");

            var estrangeira = new MetaMensal("Outra pessoa", 10m, new DateTime(2026, 6, 1), "other-user");
            metas.Metas.Add(estrangeira);
            await TestAssert.ThrowsAsync<InvalidOperationException>(
                () => service.AtualizarAsync(estrangeira.Id, new()
                {
                    Id = estrangeira.Id,
                    Nome = "Tentativa",
                    TipoDefinicao = "Percentual",
                    PercentualReceita = 10m
                }),
                "A goal belonging to another user must not be exposed");
        }

        public static async Task RoundsMixedGoalsAndKeepsHundredPercentBoundary()
        {
            var metas = new InMemoryMetaMensalRepository();
            var receitas = new InMemoryReceitaRepository();
            var service = MetaMensalTestFixtures.CreateService(metas, receitas);
            receitas.Receitas.Add(new Receita(
                "Salário",
                3333.33m,
                new DateTime(2026, 6, 5),
                new DateTime(2026, 6, 1),
                "test-user"));
            await service.CriarAsync(MetaMensalTestFixtures.CriarRequest("Proporcional", 70m));
            var fixa = await service.CriarAsync(MetaMensalTestFixtures.CriarRequestPorValor("Fixa", 1000m));

            var exato = await service.ObterResumoAsync(6, 2026);
            TestAssert.Equal(100m, exato.PercentualTotalComprometido, "Effective percentages should round to four decimals");
            TestAssert.Equal(3333.33m, exato.ValorTotalReservado, "Effective amounts should round to cents");
            TestAssert.True(!exato.Alertas.Any(alerta => alerta.Contains("ultrapassam 100%")), "Exactly 100 percent should not warn");

            await service.AtualizarAsync(fixa.Id, new()
            {
                Id = fixa.Id,
                Nome = fixa.Nome,
                TipoDefinicao = "Valor",
                PercentualReceita = null,
                ValorMeta = 1000.01m
            });
            var excedido = await service.ObterResumoAsync(6, 2026);
            TestAssert.True(excedido.PercentualTotalComprometido > 100m, "One cent above the boundary should exceed 100 percent");
            TestAssert.True(excedido.Alertas.Any(alerta => alerta.Contains("ultrapassam 100%")), "Values above 100 percent should warn");
        }
    }
}
