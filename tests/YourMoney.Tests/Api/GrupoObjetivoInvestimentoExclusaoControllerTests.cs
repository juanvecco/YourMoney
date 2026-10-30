using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class GrupoObjetivoInvestimentoExclusaoControllerTests
{
    public static async Task ExclusionReturnsNoContentAndStableConflicts()
    {
        var fake = new FakeGrupoObjetivoInvestimentoService();
        var controller = new InvestimentoGruposController(fake);
        var request = new ExcluirGrupoObjetivoRequest { VersaoOrigem = "AQ==", AcaoVinculos = "Desassociar" };
        TestAssert.True(await controller.Excluir(Guid.NewGuid(), request) is NoContentResult, "Atomic command should return 204");
        TestAssert.Equal(request, fake.LastDelete, "Controller should forward typed exclusion command");
        fake.Exception = new GrupoObjetivoConflitoException("grupo_em_uso", "em uso");
        TestAssert.True(await controller.Excluir(Guid.NewGuid(), request) is ConflictObjectResult, "In-use or stale group should return 409");
    }
}
