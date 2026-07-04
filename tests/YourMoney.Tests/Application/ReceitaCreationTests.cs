using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class ReceitaCreationTests
    {
        public static async Task CreatesNormalizedReceitaForCurrentUser()
        {
            var repository = new InMemoryReceitaRepository();
            var service = ReceitaTestFixtures.CreateService(repository);

            var response = await service.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest());

            TestAssert.Equal(1, repository.Receitas.Count, "Create should persist one receita");
            var created = repository.Receitas.Single();
            TestAssert.Equal("test-user", created.UsuarioId, "Create should use current user");
            TestAssert.Equal(5250.76m, created.Valor, "Value should round to cents");
            TestAssert.Equal(new DateTime(2026, 6, 5), created.Data, "Effective date should preserve civil day");
            TestAssert.Equal(new DateTime(2026, 5, 1), created.MesReferencia!.Value, "Reference should normalize to first day");
            TestAssert.Equal(created.Id, response.Id, "Response should identify persisted receita");
        }

        public static async Task ListsByReferenceAndFallsBackForLegacyRows()
        {
            var repository = new InMemoryReceitaRepository();
            var currentUser = new FakeCurrentUserService();
            var service = ReceitaTestFixtures.CreateService(repository, currentUser);
            await service.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest());

            var legacy = new YourMoney.Domain.Entities.Receita("Legado", 100m, new DateTime(2026, 5, 10));
            legacy.DefinirUsuario(currentUser.UserId);
            repository.Receitas.Add(legacy);

            var may = await service.ObterPorMesAnoAsync(5, 2026);
            var june = await service.ObterPorMesAnoAsync(6, 2026);

            TestAssert.Equal(2, may.Count, "Reference month should include new and legacy receitas");
            TestAssert.Equal(0, june.Count, "Effective date month should not include referenced receita");
            TestAssert.True(may.All(r => r.MesReferencia == new DateTime(2026, 5, 1)), "Read contract should expose effective reference");
        }

        public static async Task RejectsInvalidCreateData()
        {
            var service = ReceitaTestFixtures.CreateService();
            var invalidRequests = new[]
            {
                ReceitaTestFixtures.CriarRequest(descricao: " "),
                ReceitaTestFixtures.CriarRequest(descricao: new string('R', 256)),
                ReceitaTestFixtures.CriarRequest(valor: 0),
                ReceitaTestFixtures.CriarRequest(data: default(DateTime)),
                ReceitaTestFixtures.CriarRequest(mesReferencia: default(DateTime))
            };

            foreach (var request in invalidRequests)
                await TestAssert.ThrowsAsync<ArgumentException>(() => service.CriarReceitaAsync(request), "Invalid receita data should be rejected");
        }
    }
}
