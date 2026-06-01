using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class DespesaCreationTests
    {
        public static async Task CreatesExpenseFromTypedRequestWithCurrentUser()
        {
            var repository = new InMemoryDespesaRepository();
            var service = DespesaTestFixtures.CreateService(repository);
            var request = DespesaTestFixtures.CriarRequest(valor: 120.356m);

            var response = await service.CriarDespesaAsync(request);

            TestAssert.Equal(1, repository.Despesas.Count, "Create expense should persist one expense");
            var created = repository.Despesas.Single();
            TestAssert.Equal("test-user", created.UsuarioId, "Create expense should use current user as owner");
            TestAssert.Equal("Mercado", created.Descricao, "Create expense should preserve description");
            TestAssert.Equal(120.36m, created.Valor, "Create expense should round value to cents");
            TestAssert.Equal(request.MesReferencia.Date, response.MesReferencia, "Create response should preserve reference month");
            TestAssert.Equal(created.Id, response.Id, "Create response should return persisted id");
        }

        public static async Task RejectsMissingDescription()
        {
            var service = DespesaTestFixtures.CreateService();

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarDespesaAsync(DespesaTestFixtures.CriarRequest(descricao: " ")),
                "Create expense should reject missing description");
        }

        public static async Task RejectsInvalidValue()
        {
            var service = DespesaTestFixtures.CreateService();

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarDespesaAsync(DespesaTestFixtures.CriarRequest(valor: 0)),
                "Create expense should reject zero value");
        }

        public static async Task RejectsInvalidReferenceMonth()
        {
            var service = DespesaTestFixtures.CreateService();

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarDespesaAsync(DespesaTestFixtures.CriarRequest(mesReferencia: default(DateTime))),
                "Create expense should reject missing reference month");
        }

        public static async Task RejectsForeignConta()
        {
            var service = DespesaTestFixtures.CreateService(contaExists: false);

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarDespesaAsync(DespesaTestFixtures.CriarRequest()),
                "Create expense should reject account not owned by current user");
        }

        public static async Task RejectsForeignCategoria()
        {
            var service = DespesaTestFixtures.CreateService(categoriaExists: false);

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarDespesaAsync(DespesaTestFixtures.CriarRequest()),
                "Create expense should reject category not owned by current user");
        }
    }
}
