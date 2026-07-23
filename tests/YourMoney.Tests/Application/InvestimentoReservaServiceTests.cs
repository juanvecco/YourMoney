using YourMoney.Application.DTOs;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class InvestimentoReservaServiceTests
    {
        public static async Task ConsolidatesAllMonthsAndKeepsOwnerScope()
        {
            var repository = new InMemoryInvestimentoRepository();
            var service = InvestimentoTestFixtures.CreateService(repository, currentUser: new FakeCurrentUserService { UserId = "user-a" });
            await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(valorAtual: 100m, mesReferencia: new DateTime(2026, 1, 1)));
            await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(valorAtual: 250m, mesReferencia: new DateTime(2026, 7, 1), dataInvestimento: new DateTime(2026, 7, 20)));
            repository.Investimentos.Add(new Investimento("Outro", "Outro usuário", "Renda fixa", 1, 1, 999, DateTime.Today, DateTime.Today, "user-b"));

            var carteira = await service.ObterConsolidadoAsync();
            TestAssert.Equal(2, carteira.Itens.Count, "Consolidated wallet should include every owner investment across months");
            TestAssert.Equal(350m, carteira.TotalInvestido, "Consolidated total should be authoritative and unfiltered");
            TestAssert.True(carteira.Itens[0].DataInvestimento >= carteira.Itens[1].DataInvestimento, "Items should be date descending");
        }

        public static async Task CalculatesSalaryGoalsAndKeepsUnlinkedOnlyInGrandTotal()
        {
            var investments = new InMemoryInvestimentoRepository();
            var salaries = new InMemoryReceitaInvestimentoRepository();
            var salary = Salary("Salário de João", 5000m, "user-a", true);
            salaries.Recorrencias.Add(salary);
            var service = InvestimentoTestFixtures.CreateService(investments, salaries, new FakeCurrentUserService { UserId = "user-a" });

            await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(valorAtual: 15000m, receitaRecorrenteId: salary.Id));
            await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(valorAtual: 2500m));
            LinkNavigation(investments.Investimentos[0], salary);

            var carteira = await service.ObterConsolidadoAsync();
            var reserva = carteira.Reservas.Single();
            TestAssert.Equal(17500m, carteira.TotalInvestido, "Unlinked investments should remain in grand total");
            TestAssert.Equal(15000m, reserva.ValorAcumulado, "Salary reserve should sum only linked investments");
            TestAssert.Equal(30000m, reserva.MetaSeisMeses.ValorMeta, "Six-month target should use salary value");
            TestAssert.Equal(50m, reserva.MetaSeisMeses.PercentualAlcancado, "Six-month percentage should be exact");
            TestAssert.Equal(45000m, reserva.MetaDozeMeses.ValorRestante, "Twelve-month remaining amount should be exact");
        }

        public static async Task IncludesMarkedSalaryWithoutInvestmentAndPreservesCents()
        {
            var salaries = new InMemoryReceitaInvestimentoRepository();
            salaries.Recorrencias.Add(Salary("Salário sem aporte", 3333.33m, "user-a", true));
            var service = InvestimentoTestFixtures.CreateService(receitaRepository: salaries, currentUser: new FakeCurrentUserService { UserId = "user-a" });
            var reserva = (await service.ObterConsolidadoAsync()).Reservas.Single();
            TestAssert.Equal(0m, reserva.ValorAcumulado, "Marked salary should be visible without investments");
            TestAssert.Equal(19999.98m, reserva.MetaSeisMeses.ValorMeta, "Targets should preserve cents");
            TestAssert.Equal(0m, reserva.MetaSeisMeses.PercentualAlcancado, "Empty reserve should have zero percent");
        }

        public static async Task ReplaysSameOperationAndRejectsDifferentPayload()
        {
            var repository = new InMemoryInvestimentoRepository();
            var service = InvestimentoTestFixtures.CreateService(repository);
            var operation = Guid.NewGuid();
            var request = InvestimentoTestFixtures.CriarRequest(operacaoId: operation);
            var first = await service.CriarInvestimentoAsync(request);
            var replay = await service.CriarInvestimentoAsync(request);
            TestAssert.Equal(first.Id, replay.Id, "Exact replay should return original investment");
            TestAssert.Equal(1, repository.Investimentos.Count, "Exact replay should not duplicate investment");
            await TestAssert.ThrowsAsync<ConflitoOperacaoInvestimentoException>(
                () => service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(operacaoId: operation, valorAtual: 999m)),
                "Different payload should conflict for the same operation");
        }

        public static async Task PreservesHistoricalLinkButPreventsReassociation()
        {
            var investments = new InMemoryInvestimentoRepository();
            var salaries = new InMemoryReceitaInvestimentoRepository();
            var salary = Salary("Salário histórico", 4000m, "user-a", true);
            salaries.Recorrencias.Add(salary);
            var service = InvestimentoTestFixtures.CreateService(investments, salaries, new FakeCurrentUserService { UserId = "user-a" });
            var created = await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(receitaRecorrenteId: salary.Id));
            salary.Desativar();

            await service.AtualizarInvestimentoAsync(created.Id, Update(salary.Id));
            await service.AtualizarInvestimentoAsync(created.Id, Update(null));
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.AtualizarInvestimentoAsync(created.Id, Update(salary.Id)),
                "Removed inactive link should not be recreated");
        }

        public static async Task RejectsForeignSalary()
        {
            var salaries = new InMemoryReceitaInvestimentoRepository();
            var foreign = Salary("Salário alheio", 1000m, "user-b", false);
            salaries.Recorrencias.Add(foreign);
            var service = InvestimentoTestFixtures.CreateService(receitaRepository: salaries, currentUser: new FakeCurrentUserService { UserId = "user-a" });
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(receitaRecorrenteId: foreign.Id)),
                "Foreign salary should be rejected without disclosure");
        }

        private static ReceitaRecorrente Salary(string description, decimal value, string user, bool marked)
        {
            var salary = new ReceitaRecorrente(description, value, Guid.NewGuid(), NaturezaReceita.RendaDisponivel,
                true, marked, new DateTime(2026, 7, 5), new DateTime(2026, 1, 1), null, user);
            var account = new ContaFinanceira("Conta principal", user);
            typeof(ReceitaRecorrente).GetProperty(nameof(ReceitaRecorrente.ContaFinanceira))!.SetValue(salary, account);
            return salary;
        }

        private static void LinkNavigation(Investimento investment, ReceitaRecorrente salary) =>
            typeof(Investimento).GetProperty(nameof(Investimento.ReceitaRecorrente))!.SetValue(investment, salary);

        private static AtualizarInvestimentoRequest Update(Guid? salaryId) => new()
        {
            Nome = "Tesouro", Descricao = "Reserva histórica", Tipo = "Renda fixa", Quantidade = 1,
            PrecoMedio = 100, ValorAtual = 100, DataInvestimento = new DateTime(2026, 7, 22),
            MesReferencia = new DateTime(2026, 7, 1), ReceitaRecorrenteId = salaryId
        };
    }
}
