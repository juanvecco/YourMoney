using YourMoney.Application.Services;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class InvestimentoTipoAtivoTests
{
    public static async Task AcceptsEveryStableCodeAndPreservesLegacyType()
    {
        foreach (var code in Enum.GetNames<TipoAtivo>())
        {
            var service = InvestimentoTestFixtures.CreateService();
            var request = InvestimentoTestFixtures.CriarRequest(tipo: "Produto legado");
            request.TipoAtivo = code;
            var response = await service.CriarInvestimentoAsync(request);
            TestAssert.Equal(code, response.TipoAtivo, "Stable asset code should round trip");
            TestAssert.Equal("Produto legado", response.Tipo, "Legacy product/modality should remain unchanged");
        }
    }

    public static async Task ClearsAndRejectsInvalidClassification()
    {
        var repository = new InMemoryInvestimentoRepository();
        var service = InvestimentoTestFixtures.CreateService(repository);
        var create = InvestimentoTestFixtures.CriarRequest();
        create.TipoAtivo = "Acoes";
        var created = await service.CriarInvestimentoAsync(create);
        var update = new YourMoney.Application.DTOs.AtualizarInvestimentoRequest
        {
            Nome = created.Nome, Descricao = created.Descricao, Tipo = created.Tipo,
            Quantidade = created.Quantidade, PrecoMedio = created.PrecoMedio, ValorAtual = created.ValorAtual,
            DataInvestimento = created.DataInvestimento, MesReferencia = created.MesReferencia!.Value,
            ReceitaRecorrenteId = null, GrupoObjetivoId = null, TipoAtivo = null
        };
        var cleared = await service.AtualizarInvestimentoAsync(created.Id, update);
        TestAssert.True(cleared.TipoAtivo == null, "Classification should be clearable without changing legacy type");
        create.TipoAtivo = "AcaoInvalida";
        await TestAssert.ThrowsAsync<ArgumentException>(() => service.CriarInvestimentoAsync(create), "Unknown asset code should fail safely");
    }
}
