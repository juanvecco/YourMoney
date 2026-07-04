using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class DespesaCreationIsolationTests
    {
        public static async Task CreatedExpenseIsListedOnlyForAuthenticatedOwner()
        {
            var repository = new InMemoryDespesaRepository();
            var currentUser = new FakeCurrentUserService { UserId = "user-a" };
            var service = new YourMoney.Application.Services.DespesaService(
                repository,
                new InMemoryReceitaRepository(),
                new ContaFinanceiraRepositoryStub(true),
                new CategoriaRepositoryStub(true),
                currentUser);

            await service.CriarDespesaAsync(DespesaTestFixtures.CriarRequest());

            var userAExpenses = await repository.ListarAsync("user-a");
            var userBExpenses = await repository.ListarAsync("user-b");

            TestAssert.Equal(1, userAExpenses.Count, "Owner should see created expense");
            TestAssert.Equal(0, userBExpenses.Count, "Other users should not see created expense");
        }
    }
}
