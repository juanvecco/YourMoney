using YourMoney.Application.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class ReceitaCreationIsolationTests
    {
        public static async Task CreatedReceitaIsListedOnlyForOwner()
        {
            var repository = new InMemoryReceitaRepository();
            var service = new ReceitaService(repository, new FakeCurrentUserService { UserId = "user-a" });

            await service.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest());

            TestAssert.Equal(1, (await repository.ObterPorMesAnoAsync(5, 2026, "user-a")).Count, "Owner should see receita");
            TestAssert.Equal(0, (await repository.ObterPorMesAnoAsync(5, 2026, "user-b")).Count, "Other user should not see receita");
        }
    }
}
