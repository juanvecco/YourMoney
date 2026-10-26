using YourMoney.Application.DTOs;
using YourMoney.Application.Services;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class GrupoObjetivoInvestimentoServiceTests
{
    public static async Task CreatesListsAndRenamesOwnerGroups()
    {
        var repository = new InMemoryGrupoObjetivoInvestimentoRepository();
        var service = new GrupoObjetivoInvestimentoService(repository, new FakeCurrentUserService { UserId = "user-a" });
        var created = await service.CriarAsync(new CriarGrupoObjetivoRequest { Nome = " Reserva " });
        TestAssert.Equal("Reserva", created.Nome, "Create should normalize display name");
        var renamed = await service.RenomearAsync(created.Id, new RenomearGrupoObjetivoRequest { Nome = "Viver de renda", Versao = created.Versao });
        TestAssert.Equal("Viver de renda", renamed.Nome, "Owner should rename group");
        TestAssert.Equal(1, (await service.ListarAsync()).Count, "Owner should list private group");
    }

    public static async Task RejectsDuplicateAndForeignGroups()
    {
        var repository = new InMemoryGrupoObjetivoInvestimentoRepository();
        var owner = new GrupoObjetivoInvestimentoService(repository, new FakeCurrentUserService { UserId = "user-a" });
        await owner.CriarAsync(new CriarGrupoObjetivoRequest { Nome = "Reserva" });
        await TestAssert.ThrowsAsync<GrupoObjetivoConflitoException>(
            () => owner.CriarAsync(new CriarGrupoObjetivoRequest { Nome = " reserva " }),
            "Normalized duplicate should conflict for owner");

        var other = new GrupoObjetivoInvestimentoService(repository, new FakeCurrentUserService { UserId = "user-b" });
        TestAssert.Equal(0, (await other.ListarAsync()).Count, "Other user should not list owner groups");
        await other.CriarAsync(new CriarGrupoObjetivoRequest { Nome = "Reserva" });
        TestAssert.Equal(2, repository.Grupos.Count, "Different users may reuse normalized name");
    }

    public static async Task AssociatesGroupAndIncludesItInIdempotency()
    {
        var investments = new InMemoryInvestimentoRepository();
        var groups = new InMemoryGrupoObjetivoInvestimentoRepository { Investimentos = investments };
        var group = InvestimentoOrganizacaoTestFixtures.Grupo();
        await groups.AdicionarAsync(group);
        var service = InvestimentoTestFixtures.CreateService(investments, grupoRepository: groups, currentUser: new FakeCurrentUserService { UserId = "user-a" });
        var operation = Guid.NewGuid();
        var request = InvestimentoTestFixtures.CriarRequest(operacaoId: operation);
        request.GrupoObjetivoId = group.Id;
        var created = await service.CriarInvestimentoAsync(request);
        TestAssert.Equal(group.Id, created.GrupoObjetivoId, "Investment should keep selected objective");
        TestAssert.Equal(group.Nome, created.GrupoObjetivo?.Nome, "Response should include objective summary");

        request.GrupoObjetivoId = null;
        await TestAssert.ThrowsAsync<ConflitoOperacaoInvestimentoException>(
            () => service.CriarInvestimentoAsync(request),
            "Idempotency should include objective association");
    }
}
