using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;
public static class GrupoObjetivoInvestimentoCicloVidaTests
{
    public static Task ArchiveReactivatePreservesTarget()
    {
        var grupo = new GrupoObjetivoInvestimento("Carro", "owner", 150000m, new DateTime(2030, 1, 1));
        grupo.Arquivar(); grupo.Reativar();
        TestAssert.True(grupo.Ativo, "Objective must reactivate");
        TestAssert.Equal(150000m, grupo.ValorAlvo, "Lifecycle must preserve target");
        return Task.CompletedTask;
    }

    public static Task PresentationSupportsDescriptionAndSafeInternalImage()
    {
        var grupo = new GrupoObjetivoInvestimento(
            "Carro / Família",
            "owner",
            117700m,
            null,
            "City LX Sedan",
            "/assets/objectives/city-lx-sedan.jpg");

        TestAssert.Equal("City LX Sedan", grupo.Descricao, "Objective should preserve its visual description");
        TestAssert.Equal("/assets/objectives/city-lx-sedan.jpg", grupo.ImagemUrl, "Objective should preserve an internal image path");
        return TestAssert.ThrowsAsync<ArgumentException>(
            () => Task.FromResult(new GrupoObjetivoInvestimento("Carro", "owner", imagemUrl: "https://example.test/car.jpg")),
            "External image paths should not be persisted");
    }
}
