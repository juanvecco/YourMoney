using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure;

public static class PluggyInvestmentAssetLinkConfigurationTests
{
    public static Task SourceIdentityIsUniqueAndOwnerBound()
    {
        var config = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Infrastructure", "Configurations", "PluggyInvestmentAssetLinkConfiguration.cs"));
        TestAssert.True(config.Contains("UsuarioId, x.PluggyItemId, x.ExternalInvestmentId", StringComparison.Ordinal)
            && config.Contains("IsUnique()", StringComparison.Ordinal),
            "Each external investment position may only be reconciled once per owner and Pluggy item");
        TestAssert.True(config.Contains("InvestimentoId, x.UsuarioId", StringComparison.Ordinal)
            && config.Contains("HasPrincipalKey(x => new { x.Id, x.UsuarioId })", StringComparison.Ordinal),
            "A source link must not point at another user's local investment");
        return Task.CompletedTask;
    }
}
