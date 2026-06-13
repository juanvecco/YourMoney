using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class InvestimentoCreationTests
    {
        public static async Task CreatesNormalizedInvestmentForCurrentUser()
        {
            var repository = new InMemoryInvestimentoRepository();
            var service = InvestimentoTestFixtures.CreateService(repository);

            var response = await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest());

            TestAssert.Equal(1, repository.Investimentos.Count, "Create should persist one investment");
            var created = repository.Investimentos.Single();
            TestAssert.Equal("test-user", created.UsuarioId, "Create should use current user");
            TestAssert.Equal(2.51m, created.Quantidade, "Quantity should round to two decimals");
            TestAssert.Equal(153.42m, created.PrecoMedio, "Average price should round to cents");
            TestAssert.Equal(383.56m, created.ValorAtual, "Current value should round to cents");
            TestAssert.Equal(new DateTime(2026, 6, 9), created.DataInvestimento, "Date should preserve civil day");
            TestAssert.True(created.Ativo, "New investment should be active");
            TestAssert.True(created.DataResgate == null, "New investment should not have redemption date");
            TestAssert.Equal(created.Id, response.Id, "Response should identify persisted investment");
        }

        public static async Task RejectsInvalidCreateData()
        {
            var service = InvestimentoTestFixtures.CreateService();
            var invalidRequests = new[]
            {
                InvestimentoTestFixtures.CriarRequest(nome: " "),
                InvestimentoTestFixtures.CriarRequest(tipo: " "),
                InvestimentoTestFixtures.CriarRequest(quantidade: 0),
                InvestimentoTestFixtures.CriarRequest(precoMedio: 0),
                InvestimentoTestFixtures.CriarRequest(valorAtual: 0),
                InvestimentoTestFixtures.CriarRequest(dataInvestimento: default(DateTime))
            };

            foreach (var request in invalidRequests)
                await TestAssert.ThrowsAsync<ArgumentException>(() => service.CriarInvestimentoAsync(request), "Invalid investment data should be rejected");
        }

        public static async Task RejectsTextBeyondLimits()
        {
            var service = InvestimentoTestFixtures.CreateService();
            await TestAssert.ThrowsAsync<ArgumentException>(() => service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(nome: new string('N', 101))), "Long name should be rejected");
            await TestAssert.ThrowsAsync<ArgumentException>(() => service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(descricao: new string('D', 501))), "Long description should be rejected");
        }
    }
}
