using YourMoney.Application.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class InvestimentoCreationIsolationTests
    {
        public static async Task CreatedInvestmentIsListedOnlyForOwner()
        {
            var repository = new InMemoryInvestimentoRepository();
            var service = new InvestimentoService(repository, new InMemoryReceitaInvestimentoRepository(), new FakeCurrentUserService { UserId = "user-a" });

            await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest());

            TestAssert.Equal(1, (await repository.ListarAsync("user-a")).Count, "Owner should see investment");
            TestAssert.Equal(0, (await repository.ListarAsync("user-b")).Count, "Other user should not see investment");
        }

        public static async Task MonthlyQueryUsesReferenceAndOwner()
        {
            var repository = new InMemoryInvestimentoRepository();
            var receitas = new InMemoryReceitaInvestimentoRepository();
            var ownerService = new InvestimentoService(repository, receitas, new FakeCurrentUserService { UserId = "user-a" });
            var otherService = new InvestimentoService(repository, receitas, new FakeCurrentUserService { UserId = "user-b" });

            await ownerService.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest(
                dataInvestimento: new DateTime(2026, 6, 9),
                mesReferencia: new DateTime(2026, 5, 20)));

            TestAssert.Equal(1, (await ownerService.ObterPorMesAnoAsync(5, 2026)).Count, "Owner should see reference month");
            TestAssert.Equal(0, (await ownerService.ObterPorMesAnoAsync(6, 2026)).Count, "Civil date month should not override reference");
            TestAssert.Equal(0, (await otherService.ObterPorMesAnoAsync(5, 2026)).Count, "Other user should not see investment");
        }
    }
}
