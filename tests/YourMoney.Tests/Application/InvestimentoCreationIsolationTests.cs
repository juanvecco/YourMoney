using YourMoney.Application.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class InvestimentoCreationIsolationTests
    {
        public static async Task CreatedInvestmentIsListedOnlyForOwner()
        {
            var repository = new InMemoryInvestimentoRepository();
            var service = new InvestimentoService(repository, new FakeCurrentUserService { UserId = "user-a" });

            await service.CriarInvestimentoAsync(InvestimentoTestFixtures.CriarRequest());

            TestAssert.Equal(1, (await repository.ListarAsync("user-a")).Count, "Owner should see investment");
            TestAssert.Equal(0, (await repository.ListarAsync("user-b")).Count, "Other user should not see investment");
        }
    }
}
