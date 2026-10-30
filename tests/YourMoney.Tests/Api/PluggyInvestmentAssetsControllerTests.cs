using System.Text.Json;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class PluggyInvestmentAssetsControllerTests
{
    public static Task PreviewJsonUsesApplicationFieldNames()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var provider = JsonSerializer.Deserialize<PluggyInvestmentAssetDTO>("""
            {"id":"asset-1","itemId":"item-1","date":"2026-10-30T12:00:00Z",
             "value":12.5,"amountOriginal":100,"issuerCNPJ":"00000000000100"}
            """, options) ?? throw new InvalidOperationException("Missing provider position");
        TestAssert.True(provider.InvestmentId == "asset-1" && provider.CurrentUnitQuote == 12.5m
            && provider.OriginalAmount == 100m && provider.PositionDate.Year == 2026,
            "Provider JSON must still deserialize using Pluggy's field names");

        var overview = new PluggyInvestmentAssetsOverviewDTO
        {
            Assets = [new PluggyInvestmentAssetPreviewDTO
            {
                InvestmentId = provider.InvestmentId,
                ItemId = provider.ItemId,
                PositionDate = provider.PositionDate,
                CurrentUnitQuote = provider.CurrentUnitQuote,
                OriginalAmount = provider.OriginalAmount,
                IssuerCnpj = provider.IssuerCnpj
            }]
        };
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(overview, options));
        var asset = json.RootElement.GetProperty("assets")[0];
        TestAssert.True(asset.GetProperty("investmentId").GetString() == "asset-1",
            "The frontend needs investmentId to identify and reconcile a position");
        TestAssert.True(asset.GetProperty("positionDate").GetDateTimeOffset() == provider.PositionDate,
            "The frontend needs positionDate to render every position without an invalid-date error");
        TestAssert.True(asset.GetProperty("currentUnitQuote").GetDecimal() == 12.5m
            && asset.GetProperty("originalAmount").GetDecimal() == 100m
            && asset.GetProperty("issuerCnpj").GetString() == "00000000000100",
            "The preview must use normalized application names for quotes, cost and issuer");
        foreach (var alias in new[] { "id", "date", "value", "amountOriginal", "issuerCNPJ" })
            TestAssert.True(!asset.TryGetProperty(alias, out _),
                $"Provider alias {alias} must not replace the application response contract");
        return Task.CompletedTask;
    }

    public static Task PreviewIsProtectedAndUsesAssetEndpoint()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Controllers", "PluggyController.cs"));
        TestAssert.True(source.Contains("[Authorize]", StringComparison.Ordinal),
            "Open Finance investment asset preview must require authorization");
        TestAssert.True(source.Contains("[HttpGet(\"investments/preview\")]", StringComparison.Ordinal),
            "Assets need a dedicated preview route");
        TestAssert.True(source.Contains("ObterAtivosInvestimentoAsync", StringComparison.Ordinal),
            "Preview should use the investment asset service, not generic transactions");
        TestAssert.True(source.Contains("[FromQuery] bool retryFailed", StringComparison.Ordinal),
            "The preview should accept an explicit failed-sync retry without altering successful refresh limits");
        TestAssert.True(source.Contains("schema_migration_required", StringComparison.Ordinal)
            && source.Contains("OpenFinanceInvestmentAssets", StringComparison.Ordinal),
            "A missing investment asset schema should return an actionable migration response");
        return Task.CompletedTask;
    }

    public static Task CacheAndOwnerRulesArePersisted()
    {
        var service = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var entity = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Domain", "Entities", "PluggyInvestmentAssetSnapshot.cs"));
        TestAssert.True(service.Contains("InvestmentAssetsLastAttemptAt", StringComparison.Ordinal)
            && service.Contains("TimeSpan.FromHours(24)", StringComparison.Ordinal),
            "Investment provider requests must be throttled with persisted per-item attempt time");
        TestAssert.True(service.Contains("UsuarioId == usuarioId", StringComparison.Ordinal)
            && service.Contains("SnapshotToken", StringComparison.Ordinal),
            "Asset reads and reconciliation must remain owner-scoped and use a snapshot token");
        TestAssert.True(entity.Contains("IsCurrent", StringComparison.Ordinal)
            && entity.Contains("FetchedAt", StringComparison.Ordinal),
            "The last successful normalized snapshot must be persisted");
        return Task.CompletedTask;
    }
}
