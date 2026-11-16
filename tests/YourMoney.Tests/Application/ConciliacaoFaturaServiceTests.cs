using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class ConciliacaoFaturaServiceTests
{
    public static async Task PreservesCentsAndAllExpensesWithoutPagination()
    {
        var despesas = new InMemoryDespesaRepository();
        despesas.Despesas.Add(new Despesa("A", 10.01m, new DateTime(2026, 11, 3), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user"));
        despesas.Despesas.Add(new Despesa("B", 20.02m, new DateTime(2026, 11, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user"));
        despesas.Despesas.Add(new Despesa("Outro usuário", 50m, new DateTime(2026, 11, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "other-user"));
        despesas.Despesas.Add(new Despesa("Outro mês", 50m, new DateTime(2026, 10, 4), DespesaTestFixtures.ContaId, DespesaTestFixtures.CategoriaId, "test-user"));
        var service = Create(despesas);

        var result = await service.SalvarAsync(DespesaTestFixtures.ContaId, 11, 2026, 50.05m);
        TestAssert.Equal(30.03m, result.TotalCadastrado, "Bill total must include all owner expenses with cents");
        TestAssert.Equal(20.02m, result.Diferenca, "Difference must keep cents");
        TestAssert.Equal(2, result.QuantidadePendentes, "Pending count must use all expenses");
    }

    public static async Task DistinguishesMissingBillFromZeroAndScopesOwner()
    {
        var repo = new InMemoryFaturaContaRepository();
        var service = Create(new InMemoryDespesaRepository(), repo);
        var missing = await service.ObterAsync(DespesaTestFixtures.ContaId, 11, 2026);
        TestAssert.True(missing.ValorInformado is null && missing.Diferenca is null, "Missing bill must be null");
        var zero = await service.SalvarAsync(DespesaTestFixtures.ContaId, 11, 2026, 0m);
        TestAssert.Equal(0m, zero.ValorInformado, "Zero bill is valid");
        TestAssert.Equal(1, repo.Faturas.Count, "Save must create one row");
        TestAssert.Equal("test-user", repo.Faturas[0].UsuarioId, "Owner must come from session");
        await service.SalvarAsync(DespesaTestFixtures.ContaId, 12, 2026, 50m);
        await service.SalvarAsync(DespesaTestFixtures.ContaLazerId, 11, 2026, 75m);
        TestAssert.Equal(3, repo.Faturas.Count, "Different accounts and months need separate bills");
        TestAssert.Equal(0m, (await service.ObterAsync(DespesaTestFixtures.ContaId, 11, 2026)).ValorInformado,
            "Changing another account or month must preserve the first bill");
        await TestAssert.ThrowsAsync<ArgumentException>(() => service.SalvarAsync(DespesaTestFixtures.ContaId, 11, 2026, -1m), "Negative bill must fail");
    }

    private static ConciliacaoFaturaService Create(InMemoryDespesaRepository despesas, InMemoryFaturaContaRepository? faturas = null) =>
        new(faturas ?? new(), despesas, new ContaFinanceiraRepositoryStub(true), new FakeCurrentUserService());

    private sealed class InMemoryFaturaContaRepository : IFaturaContaRepository
    {
        public List<FaturaConta> Faturas { get; } = new();
        public Task<FaturaConta?> ObterAsync(Guid contaId, DateTime mes, string usuarioId) =>
            Task.FromResult(Faturas.FirstOrDefault(f => f.IdContaFinanceira == contaId && f.MesReferencia == mes && f.UsuarioId == usuarioId));
        public Task SalvarAsync(FaturaConta fatura)
        {
            if (!Faturas.Contains(fatura)) Faturas.Add(fatura);
            return Task.CompletedTask;
        }
    }
}
