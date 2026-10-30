using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class GrupoObjetivoInvestimentoControllerTests
{
    public static async Task CrudReturnsProtectedTypedContracts()
    {
        var fake = new FakeGrupoObjetivoInvestimentoService();
        var controller = new InvestimentoGruposController(fake);
        TestAssert.True(typeof(InvestimentoGruposController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any(), "Group controller should require authorization");
        TestAssert.True(await controller.Listar() is OkObjectResult, "List should return 200");
        TestAssert.True(await controller.Criar(new CriarGrupoObjetivoRequest { Nome = "Reserva" }) is CreatedAtActionResult, "Create should return 201");
        TestAssert.True(await controller.Renomear(Guid.NewGuid(), new RenomearGrupoObjetivoRequest { Nome = "Renda", Versao = "AQ==" }) is OkObjectResult, "Rename should return 200");
    }

    public static async Task MapsValidationNotFoundConflictAndFailure()
    {
        var fake = new FakeGrupoObjetivoInvestimentoService { Exception = new ArgumentException("inválido") };
        var controller = new InvestimentoGruposController(fake);
        TestAssert.True(await controller.Criar(new CriarGrupoObjetivoRequest()) is BadRequestObjectResult, "Validation should return 400");
        fake.Exception = new KeyNotFoundException();
        TestAssert.True(await controller.Obter(Guid.NewGuid()) is NotFoundObjectResult, "Foreign or missing should return 404");
        fake.Exception = new GrupoObjetivoConflitoException("grupo_nome_duplicado", "duplicado");
        TestAssert.True(await controller.Renomear(Guid.NewGuid(), new RenomearGrupoObjetivoRequest()) is ConflictObjectResult, "Duplicate or stale should return 409");
        fake.Exception = new Exception();
        TestAssert.Equal(500, (await controller.Listar() as ObjectResult)?.StatusCode, "Unexpected failure should return 500");
    }
}
