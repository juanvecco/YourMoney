using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class ReceitaClassificationTests
    {
        public static async Task CreatesClassifiedReceitaAndEligibleTotal()
        {
            var repository = new InMemoryReceitaRepository();
            var service = ReceitaTestFixtures.CreateService(repository);

            var salario = await service.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest(valor: 5000m, natureza: "RendaDisponivel"));
            var vale = await service.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest(descricao: "Vale alimentação", valor: 800m, natureza: "EntradaVinculadaDespesa"));

            var elegivel = await repository.GetTotalElegivelMetasByMesAnoAsync(5, 2026, "test-user");
            var bruto = await repository.GetTotalBrutoByMesAnoAsync(5, 2026, "test-user");

            TestAssert.True(salario.ConsideraNasMetas, "Available income should count for goals");
            TestAssert.True(!vale.ConsideraNasMetas, "Restricted income should not count for goals");
            TestAssert.Equal(5000m, elegivel, "Eligible revenue should include only available income");
            TestAssert.Equal(5800m, bruto, "Gross revenue should include all entries");
        }

        public static async Task UpdatesClassificationAndClearsInvalidLink()
        {
            var repository = new InMemoryReceitaRepository();
            var service = ReceitaTestFixtures.CreateService(repository);
            var created = await service.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest(valor: 800m, natureza: "EntradaVinculadaDespesa"));

            var updated = await service.AtualizarReceitaAsync(created.Id, new()
            {
                Id = created.Id,
                Descricao = "Salário ajustado",
                Valor = 800m,
                Data = created.Data,
                MesReferencia = created.MesReferencia,
                Natureza = "RendaDisponivel"
            });

            TestAssert.Equal("RendaDisponivel", updated.Natureza, "Update should change classification");
            TestAssert.True(updated.ConsideraNasMetas, "Updated available income should count for goals");
        }

        public static async Task RejectsInvalidNature()
        {
            var service = ReceitaTestFixtures.CreateService();

            await TestAssert.ThrowsAsync<ArgumentException>(
                () => service.CriarReceitaAsync(ReceitaTestFixtures.CriarRequest(natureza: "LivreParaTudo")),
                "Invalid nature should be rejected");
        }
    }
}
