using YourMoney.Application.DTOs;
using YourMoney.Application.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class GrupoObjetivoInvestimentoExclusaoTests
{
    public static async Task RequiresExplicitDecisionForUsedGroup()
    {
        var investments = new InMemoryInvestimentoRepository();
        var groups = new InMemoryGrupoObjetivoInvestimentoRepository { Investimentos = investments };
        var source = InvestimentoOrganizacaoTestFixtures.Grupo();
        await groups.AdicionarAsync(source);
        investments.Investimentos.Add(InvestimentoOrganizacaoTestFixtures.Investimento(grupo: source));
        var service = new GrupoObjetivoInvestimentoService(groups, new FakeCurrentUserService { UserId = "user-a" });
        await TestAssert.ThrowsAsync<GrupoObjetivoConflitoException>(
            () => service.ExcluirAsync(source.Id, new ExcluirGrupoObjetivoRequest { VersaoOrigem = Convert.ToBase64String(source.Versao), AcaoVinculos = "ExigirVazio" }),
            "Used group should require transfer or unlink");
    }

    public static async Task UnlinksOrTransfersWithoutDeletingInvestments()
    {
        var investments = new InMemoryInvestimentoRepository();
        var groups = new InMemoryGrupoObjetivoInvestimentoRepository { Investimentos = investments };
        var source = InvestimentoOrganizacaoTestFixtures.Grupo(nome: "Origem");
        var destination = InvestimentoOrganizacaoTestFixtures.Grupo(nome: "Destino");
        await groups.AdicionarAsync(source);
        await groups.AdicionarAsync(destination);
        var investment = InvestimentoOrganizacaoTestFixtures.Investimento(grupo: source, valor: 123.45m);
        investments.Investimentos.Add(investment);
        var service = new GrupoObjetivoInvestimentoService(groups, new FakeCurrentUserService { UserId = "user-a" });
        await service.ExcluirAsync(source.Id, new ExcluirGrupoObjetivoRequest
        {
            VersaoOrigem = Convert.ToBase64String(source.Versao), AcaoVinculos = "Transferir",
            GrupoDestinoId = destination.Id, VersaoDestino = Convert.ToBase64String(destination.Versao)
        });
        TestAssert.Equal(1, investments.Investimentos.Count, "Group deletion must never delete investments");
        TestAssert.Equal(destination.Id, investment.GrupoObjetivoInvestimentoId, "Transfer should update only objective link");
        TestAssert.Equal(123.45m, investment.ValorAtual, "Transfer should preserve money");
    }

    public static async Task RejectsInvalidSelfAndForeignDestinations()
    {
        var groups = new InMemoryGrupoObjetivoInvestimentoRepository();
        var source = InvestimentoOrganizacaoTestFixtures.Grupo();
        await groups.AdicionarAsync(source);
        var service = new GrupoObjetivoInvestimentoService(groups, new FakeCurrentUserService { UserId = "user-a" });
        await TestAssert.ThrowsAsync<ArgumentException>(() => service.ExcluirAsync(source.Id, new ExcluirGrupoObjetivoRequest
        {
            VersaoOrigem = Convert.ToBase64String(source.Versao), AcaoVinculos = "Transferir",
            GrupoDestinoId = source.Id, VersaoDestino = Convert.ToBase64String(source.Versao)
        }), "Self destination should fail");
        await TestAssert.ThrowsAsync<KeyNotFoundException>(() => service.ExcluirAsync(source.Id, new ExcluirGrupoObjetivoRequest
        {
            VersaoOrigem = Convert.ToBase64String(source.Versao), AcaoVinculos = "Transferir",
            GrupoDestinoId = Guid.NewGuid(), VersaoDestino = "AQ=="
        }), "Missing or foreign destination should be hidden");
    }
}
