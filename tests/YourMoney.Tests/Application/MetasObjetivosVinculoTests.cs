using YourMoney.Application.DTOs;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;
public static class MetasObjetivosVinculoTests
{
    public static Task ContractsPreserveOwnerDerivedCanonicalLink()
    {
        TestAssert.True(typeof(CriarMetaMensalDTO).GetProperty("GrupoObjetivoInvestimentoId") != null, "Create contract must expose objective link");
        TestAssert.True(typeof(CriarMetaMensalDTO).GetProperty("UsuarioId") == null, "Client must not supply owner");
        var grupo = new GrupoObjetivoInvestimento("Aluguel", "owner");
        var meta = new MetaMensal("Guardar", TipoDefinicaoMeta.Valor, null, 1000m, DateTime.Today, "owner");
        meta.AssociarGrupoObjetivo(grupo.Id);
        TestAssert.Equal(grupo.Id, meta.GrupoObjetivoInvestimentoId, "Link must use canonical objective id");
        return Task.CompletedTask;
    }
}
