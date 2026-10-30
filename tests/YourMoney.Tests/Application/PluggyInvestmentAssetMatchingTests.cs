using YourMoney.Api.Services;
using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class PluggyInvestmentAssetMatchingTests
{
    public static Task LegacyDescriptionMatchesWithoutIdentifiersOrEqualValues()
    {
        var manual = Manual("Reserva de emergência", "CDB AURORA - liquidez diária", 900m);
        var matches = PluggyInvestmentAssetMatcher.FindCandidates(Asset(), [manual]);
        TestAssert.True(matches.Count == 1 && matches[0].Investment.Id == manual.Id,
            "A legacy description must match without code/ISIN or matching provider/local type labels");
        TestAssert.True(matches[0].Reason == "Descrição igual"
            && matches[0].ValueDifference == 100m && matches[0].ValueDifferencePercent == 11.11m,
            "The comparison must explain the text match and the signed difference from the manual value");
        TestAssert.True(manual.ValorAtual == 900m && manual.CodigoAtivo is null && manual.Isin is null,
            "Reading suggestions must not update the manual investment or its identifiers");
        return Task.CompletedTask;
    }

    public static Task SimilarDescriptionsAreRankedByValueWithoutRejectingOldBalances()
    {
        var distant = Manual("Reserva antiga", "CDB do Banco Aurora Liquidez Diária", 50m);
        var nearby = Manual("Reserva recente", "CDB do Banco Aurora Liquidez Diária", 990m);
        var matches = PluggyInvestmentAssetMatcher.FindCandidates(Asset(), [distant, nearby]);
        TestAssert.True(matches.Count == 2 && matches[0].Investment.Id == nearby.Id
            && matches[1].Investment.Id == distant.Id && matches[0].Reason.StartsWith("Descrição semelhante"),
            "Text must qualify the match; proximity orders comparable candidates but is not a cutoff");
        return Task.CompletedTask;
    }

    public static Task SimilarValuesAndGenericWordsDoNotIdentifyInvestments()
    {
        var other = Manual("CDB Banco Horizonte", "CDB Banco Horizonte", 1000m);
        var generic = Manual("Fundo Renda Fixa Horizonte", "Fundo Renda Fixa Horizonte", 1000m);
        var otherOwner = Manual("Reserva", "CDB Aurora Liquidez Diária", 1000m);
        otherOwner.DefinirUsuario("another-owner");
        var inactive = Manual("Reserva resgatada", "CDB Aurora Liquidez Diária", 1000m);
        inactive.Resgatar();
        TestAssert.True(PluggyInvestmentAssetMatcher.FindCandidates(Asset(), [other, otherOwner, inactive]).Count == 0,
            "Amount alone must not suggest unrelated, foreign-owner or inactive investments");
        TestAssert.True(PluggyInvestmentAssetMatcher.FindCandidates(
            Asset("Fundo Renda Fixa Aurora"), [generic]).Count == 0,
            "Shared generic product words must not qualify different funds");
        TestAssert.True(PluggyInvestmentAssetMatcher.FindCandidates(
            Asset("Tesouro Selic 2029"), [Manual("Reserva", "Tesouro Selic 2031", 1000m)]).Count == 0,
            "Different maturities must not be matched merely by similar product names");
        return Task.CompletedTask;
    }

    public static Task IdentifiersTakePrecedenceWhenAvailableAndCurrencyIsPreserved()
    {
        var text = Manual("Reserva", "CDB Aurora Liquidez Diária", 999m);
        var identified = Manual("Nome personalizado", "Outro texto manual", 100m);
        identified.AtualizarCodigoAtivo("CDB-A");
        var conflicting = Manual("CDB Aurora Liquidez Diária", "CDB Aurora Liquidez Diária", 1000m);
        conflicting.AtualizarCodigoAtivo("CDB-B");
        var matches = PluggyInvestmentAssetMatcher.FindCandidates(Asset(code: "CDB-A"), [text, identified, conflicting]);
        TestAssert.True(matches.Count == 2 && matches[0].Investment.Id == identified.Id,
            "Available exact identifiers must precede textual suggestions; conflicting identifiers must be excluded");
        var foreign = PluggyInvestmentAssetMatcher.FindCandidates(Asset(currency: "USD"), [text]);
        TestAssert.True(foreign.Count == 1 && foreign[0].ValueDifference is null
            && foreign[0].ValueDifferencePercent is null,
            "A foreign balance must not be compared or converted to the manual BRL value");
        return Task.CompletedTask;
    }

    public static Task ConfirmedAssociationCompletesOnlyMissingIdentifiers()
    {
        var manual = Manual("Reserva de emergência", "Minha descrição", 900m);
        var date = manual.DataInvestimento;
        var month = manual.MesReferencia;
        TestAssert.True(manual.CompletarIdentificadoresAtivo(" CDB-A ", " br0000000001 "),
            "An association may complete identifiers omitted in a legacy record");
        TestAssert.True(manual.CodigoAtivo == "CDB-A" && manual.Isin == "BR0000000001",
            "Identifiers must follow the existing domain normalization rules");
        TestAssert.True(!manual.CompletarIdentificadoresAtivo("CDB-B", "BR0000000002")
            && !manual.CompletarIdentificadoresAtivo(null, null),
            "Repeated association must preserve existing identifiers rather than replace or clear them");
        TestAssert.True(manual.Nome == "Reserva de emergência" && manual.Descricao == "Minha descrição"
            && manual.Tipo == "Renda fixa" && manual.ValorAtual == 900m && manual.Quantidade == 2m
            && manual.PrecoMedio == 400m && manual.DataInvestimento == date && manual.MesReferencia == month,
            "Completing identity metadata must preserve financial and historical manual data");
        var matchedAgain = PluggyInvestmentAssetMatcher.FindCandidates(
            Asset("Outro nome da instituição", code: "CDB-A"), [manual]);
        TestAssert.True(matchedAgain.Count == 1 && matchedAgain[0].Reason == "Código do ativo igual",
            "The completed identifier should enable subsequent recognition even if the description changes");
        return Task.CompletedTask;
    }

    private static Investimento Manual(string name, string description, decimal value) => new(
        name, description, "Renda fixa", 2m, 400m, value,
        new DateTime(2026, 1, 10), new DateTime(2026, 1, 1), "owner");

    private static PluggyInvestmentAssetSnapshot Asset(
        string name = "CDB Aurora Liquidez Diária", string? code = null, string currency = "BRL") => new(
        "owner", Guid.NewGuid(), "item-1", "asset-1", name, "FIXED_INCOME", null, code, null,
        1000m, currency, 2m, 500m, null, DateTimeOffset.UtcNow, null, null, null, "ACTIVE", DateTimeOffset.UtcNow);
}
