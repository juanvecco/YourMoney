using System.Reflection;
using YourMoney.Api.Services;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class PluggyInvestmentOverviewTests
{
    private static readonly MethodInfo MatchingMethod = typeof(PluggyService).GetMethod(
        "ObterMotivosCorrespondencia",
        BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Investment matching rule was not found.");

    public static Task SameAmountWithinOneDaySuggestsInvestmentCandidate()
    {
        var reasons = MatchingMethod.Invoke(null, new object?[]
        {
            "Tesouro Selic",
            new DateOnly(2026, 10, 28),
            125.50m,
            "APLICACAO TESOURO SELIC",
            new DateOnly(2026, 10, 29),
            125.50m
        }) as IReadOnlyList<string>;

        TestAssert.True(reasons is not null, "Same amount within one day should remain a review suggestion");
        var matches = reasons ?? Array.Empty<string>();
        TestAssert.True(matches.Contains("Mesmo valor em centavos"), "The exact amount should be explained");
        TestAssert.True(matches.Contains("Data com diferença de um dia"), "The date difference should be explained");
        return Task.CompletedTask;
    }

    public static Task InvestmentCandidateCanBeMatchedByDescriptionWithAmountCorrection()
    {
        var reasons = MatchingMethod.Invoke(null, new object?[]
        {
            "ETF IVVB11",
            new DateOnly(2026, 10, 29),
            500m,
            "ETF IVVB11",
            new DateOnly(2026, 10, 29),
            520m
        }) as IReadOnlyList<string>;

        TestAssert.True(reasons is not null, "Same-day description should suggest a candidate for review");
        var matches = reasons ?? Array.Empty<string>();
        TestAssert.True(matches.Contains("Valor diferente"), "The amount difference should be visible");
        return Task.CompletedTask;
    }

    public static Task InvestmentOverviewScopesAndReturnsEveryCandidate()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var start = source.IndexOf("private async Task AnexarSituacaoCadastroAsync(", StringComparison.Ordinal);
        var end = source.IndexOf("private static IReadOnlyList<string>? ObterMotivosCorrespondencia(", start, StringComparison.Ordinal);
        TestAssert.True(start >= 0 && end > start, "The investment overview projection should be present");
        var projection = source[start..end];
        TestAssert.True(projection.Contains("investment.UsuarioId == usuarioId", StringComparison.Ordinal),
            "Investment candidates must be scoped to the authenticated owner");
        TestAssert.True(projection.Contains("source.PossibleDuplicateCandidates = matchingCandidates", StringComparison.Ordinal),
            "The overview should return every matching investment for review");
        TestAssert.True(projection.Contains("PodeReconciliarComoInvestimento(source)", StringComparison.Ordinal),
            "Ineligible transactions should remain unavailable for investment actions");
        return Task.CompletedTask;
    }
}
