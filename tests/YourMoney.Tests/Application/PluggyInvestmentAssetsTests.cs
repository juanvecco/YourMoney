using YourMoney.Tests.Fixtures;
using YourMoney.Api.Services;

namespace YourMoney.Tests.Application;

public static class PluggyInvestmentAssetsTests
{
    public static Task PreviewUsesInvestmentEndpointAndPagePagination()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var start = source.IndexOf("ConsultarPosicoesInvestimentoAsync(", StringComparison.Ordinal);
        TestAssert.True(start >= 0,
            "A separate Pluggy investment position query should exist before the bank transaction query");
        var end = source.IndexOf("ListarTransacoesParaContasAsync(", start, StringComparison.Ordinal);
        TestAssert.True(end > start,
            "A separate Pluggy investment position query should exist before the bank transaction query");
        var method = source[start..end];
        TestAssert.True(method.Contains("\"investments\"", StringComparison.Ordinal)
            && method.Contains("?itemId=", StringComparison.Ordinal),
            "Investment positions must be queried by the server-owned Pluggy item id");
        TestAssert.True(method.Contains("TotalPages", StringComparison.Ordinal),
            "All page-numbered provider results must be loaded");
        TestAssert.True(!method.Contains("v2/transactions", StringComparison.Ordinal),
            "The investment position query must not call the generic account transaction endpoint");
        return Task.CompletedTask;
    }

    public static Task PositionContractPreservesProviderSemantics()
    {
        var path = RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Application", "DTOs", "PluggyInvestmentAssetDTO.cs");
        TestAssert.True(File.Exists(path), "A separate typed provider investment asset DTO should exist");
        var source = File.ReadAllText(path);
        foreach (var property in new[]
        {
            "InvestmentId", "ItemId", "Balance", "CurrencyCode", "Quantity",
            "CurrentUnitQuote", "OriginalAmount", "PositionDate", "PurchaseDate", "Status"
        })
        {
            TestAssert.True(source.Contains(property, StringComparison.Ordinal),
                $"The investment asset DTO should preserve {property}");
        }

        return Task.CompletedTask;
    }

    public static Task FailedRefreshUsesControlledRetryWindow()
    {
        var now = new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
        var lastSuccess = now.AddHours(-2);
        var lastAttempt = now.AddSeconds(-30);
        var earlyRetry = PluggyInvestmentAssetRefreshPolicy.Evaluate(
            "stale", lastAttempt, lastSuccess, now, retryFailed: true,
            TimeSpan.FromHours(24), TimeSpan.FromMinutes(1));

        TestAssert.True(!earlyRetry.ShouldSynchronize,
            "An explicit retry before the one-minute failure cooldown must not call the provider");
        TestAssert.True(earlyRetry.RetryAfterAt == lastAttempt.AddMinutes(1),
            "A failed connection should report its next eligible retry time");
        TestAssert.True(earlyRetry.NextRefreshAt == lastSuccess.AddHours(24),
            "A failed attempt must not move the 24-hour success refresh time");

        var eligibleRetry = PluggyInvestmentAssetRefreshPolicy.Evaluate(
            "stale", lastAttempt, lastSuccess, lastAttempt.AddMinutes(1), retryFailed: true,
            TimeSpan.FromHours(24), TimeSpan.FromMinutes(1));
        TestAssert.True(eligibleRetry.ShouldSynchronize,
            "An explicit retry should be allowed when the short failure cooldown expires");

        var service = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var syncMethod = service[service.IndexOf("SincronizarAtivosInvestimentoAsync(", StringComparison.Ordinal)..];
        TestAssert.True(syncMethod.IndexOf("ConsultarPosicoesInvestimentoAsync(", StringComparison.Ordinal)
            < syncMethod.IndexOf("snapshot.MarcarAusente()", StringComparison.Ordinal),
            "A previous snapshot must not be marked absent before all provider pages have loaded");
        return Task.CompletedTask;
    }

    public static Task SuccessfulAssetRefreshCannotBeForced()
    {
        var now = new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);
        var recentSuccess = PluggyInvestmentAssetRefreshPolicy.Evaluate(
            "current", now.AddMinutes(-5), now.AddHours(-2), now, retryFailed: true,
            TimeSpan.FromHours(24), TimeSpan.FromMinutes(1));
        TestAssert.True(!recentSuccess.ShouldSynchronize,
            "A failed-retry flag must not bypass the daily window after a successful sync");

        var expiredSuccess = PluggyInvestmentAssetRefreshPolicy.Evaluate(
            "current", now.AddHours(-25), now.AddHours(-25), now, retryFailed: false,
            TimeSpan.FromHours(24), TimeSpan.FromMinutes(1));
        TestAssert.True(expiredSuccess.ShouldSynchronize,
            "A successful snapshot should become eligible after the daily refresh interval");
        return Task.CompletedTask;
    }
}
