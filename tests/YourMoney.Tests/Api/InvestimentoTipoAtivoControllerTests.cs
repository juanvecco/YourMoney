using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class InvestimentoTipoAtivoControllerTests
{
    public static async Task AdditiveTypeFieldsRoundTripOnCommands()
    {
        var fake = new FakeInvestimentoService
        {
            CreateResponse = new CriarInvestimentoResponse { CriadoAgora = true, TipoAtivo = "Criptomoeda" },
            UpdateResponse = new InvestimentoResponse { TipoAtivo = null }
        };
        var controller = new InvestimentoController(fake);
        var request = InvestimentoTestFixtures.CriarRequest();
        request.TipoAtivo = "Criptomoeda";
        await controller.AdicionarInvestimento(request);
        TestAssert.Equal("Criptomoeda", fake.LastRequest?.TipoAtivo, "Create controller should bind stable code");
        var update = new AtualizarInvestimentoRequest { TipoAtivo = null };
        await controller.AtualizarInvestimento(Guid.NewGuid(), update);
        TestAssert.True(fake.LastUpdateRequest?.TipoAtivo == null, "Update controller should bind explicit null");
    }
}
