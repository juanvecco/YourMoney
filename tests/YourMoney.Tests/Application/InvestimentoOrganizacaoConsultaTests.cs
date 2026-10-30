using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class InvestimentoOrganizacaoConsultaTests
{
    public static async Task FiltersWithAndAndExactBuckets()
    {
        var repository = new InMemoryInvestimentoRepository();
        var groups = new InMemoryGrupoObjetivoInvestimentoRepository { Investimentos = repository };
        var reserva = InvestimentoOrganizacaoTestFixtures.Grupo(nome: "Reserva");
        var renda = InvestimentoOrganizacaoTestFixtures.Grupo(nome: "Renda");
        await groups.AdicionarAsync(reserva);
        await groups.AdicionarAsync(renda);
        repository.Investimentos.AddRange([
            InvestimentoOrganizacaoTestFixtures.Investimento(valor: 100.11m, grupo: reserva, tipoAtivo: TipoAtivo.RendaFixa),
            InvestimentoOrganizacaoTestFixtures.Investimento(valor: 200.22m, grupo: reserva, tipoAtivo: TipoAtivo.Acoes),
            InvestimentoOrganizacaoTestFixtures.Investimento(valor: 300.33m, grupo: renda, tipoAtivo: TipoAtivo.RendaFixa),
            InvestimentoOrganizacaoTestFixtures.Investimento(valor: 400.44m)
        ]);
        var service = InvestimentoTestFixtures.CreateService(repository, grupoRepository: groups, currentUser: new FakeCurrentUserService { UserId = "user-a" });
        var result = await service.ObterConsolidadoAsync(InvestimentoOrganizacaoTestFixtures.Filtro(reserva.Id, tipo: "RendaFixa"));
        TestAssert.Equal(1001.10m, result.TotalInvestido, "Global total should ignore filters");
        TestAssert.Equal(100.11m, result.TotalFiltrado, "AND intersection should preserve exact cents");
        TestAssert.Equal(1, result.Itens.Count, "Combined filters should not double count");
        TestAssert.Equal(result.TotalFiltrado, result.SubtotaisPorGrupo.Sum(s => s.ValorAtual), "Group buckets should decompose filtered total");
        TestAssert.Equal(result.TotalFiltrado, result.SubtotaisPorTipoAtivo.Sum(s => s.ValorAtual), "Type buckets should decompose filtered total");
    }

    public static async Task ValidatesContradictionsNullBucketsAndForeignGroup()
    {
        var repository = new InMemoryInvestimentoRepository();
        repository.Investimentos.Add(InvestimentoOrganizacaoTestFixtures.Investimento(valor: 10.01m));
        var groups = new InMemoryGrupoObjetivoInvestimentoRepository();
        var service = InvestimentoTestFixtures.CreateService(repository, grupoRepository: groups, currentUser: new FakeCurrentUserService { UserId = "user-a" });
        var nulls = await service.ObterConsolidadoAsync(InvestimentoOrganizacaoTestFixtures.Filtro(semGrupo: true, naoClassificado: true));
        TestAssert.Equal(10.01m, nulls.TotalFiltrado, "Explicit absence filters should select historical nulls");
        await TestAssert.ThrowsAsync<ArgumentException>(
            () => service.ObterConsolidadoAsync(InvestimentoOrganizacaoTestFixtures.Filtro(Guid.NewGuid(), semGrupo: true)),
            "Contradictory group filter should fail");
        await TestAssert.ThrowsAsync<KeyNotFoundException>(
            () => service.ObterConsolidadoAsync(InvestimentoOrganizacaoTestFixtures.Filtro(Guid.NewGuid())),
            "Unavailable group should be hidden as not found");
    }

    public static async Task HandlesPlannedMaximumDeterministically()
    {
        var repository = new InMemoryInvestimentoRepository();
        var groups = new InMemoryGrupoObjetivoInvestimentoRepository { Investimentos = repository };
        for (var groupIndex = 0; groupIndex < 100; groupIndex++)
        {
            var group = InvestimentoOrganizacaoTestFixtures.Grupo(nome: $"Objetivo {groupIndex:D3}");
            await groups.AdicionarAsync(group);
            for (var investmentIndex = 0; investmentIndex < 10; investmentIndex++)
                repository.Investimentos.Add(InvestimentoOrganizacaoTestFixtures.Investimento(valor: 0.01m, grupo: group));
        }
        var service = InvestimentoTestFixtures.CreateService(repository, grupoRepository: groups, currentUser: new FakeCurrentUserService { UserId = "user-a" });
        var result = await service.ObterConsolidadoAsync();
        TestAssert.Equal(1000, result.Itens.Count, "Consolidated read should handle planned maximum");
        TestAssert.Equal(100, result.SubtotaisPorGrupo.Count, "Consolidated read should handle one hundred objective groups");
        TestAssert.Equal(10.00m, result.TotalFiltrado, "Thousand cent values should remain exact");
    }
}
