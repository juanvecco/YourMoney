using System.Reflection;
using YourMoney.Api.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class PluggyExpenseCandidateMatchingTests
{
    private static readonly MethodInfo MatchMethod = typeof(PluggyService).GetMethod(
        "ObterMotivosCorrespondencia",
        BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Expense candidate matching rule was not found.");

    public static Task SameDayNormalizedDescriptionAndCentsAreExplained()
    {
        var reasons = Match(
            "Mercadão do Bairro!",
            new DateOnly(2026, 5, 12),
            42.50m,
            "MERCADAO DO BAIRRO",
            new DateOnly(2026, 5, 12),
            42.504m);

        var matches = reasons ?? throw new InvalidOperationException(
            "Normalized description and rounded cents should suggest a candidate.");
        TestAssert.True(matches.Contains("Mesma data"), "The matching date should be explained");
        TestAssert.True(matches.Contains("Mesmo valor em centavos"), "The matching cents should be explained");
        TestAssert.True(matches.Contains("Descrição igual após normalização"), "The normalized description should be explained");
        return Task.CompletedTask;
    }

    public static Task SameAmountWithinOneDaySuggestsDifferentDescription()
    {
        var reasons = Match(
            "Compra manual",
            new DateOnly(2026, 5, 13),
            42.50m,
            "MERCEARIA CENTRAL",
            new DateOnly(2026, 5, 12),
            42.50m);

        var matches = reasons ?? throw new InvalidOperationException(
            "Same amount within one day should suggest a candidate for review.");
        TestAssert.True(matches.Contains("Data com diferença de um dia"), "The date difference should be explained");
        TestAssert.True(matches.Contains("Descrição diferente"), "The description difference should be explained");
        return Task.CompletedTask;
    }

    public static Task SameDayDescriptionCanSuggestAmountCorrection()
    {
        var reasons = Match(
            "Farmácia São José",
            new DateOnly(2026, 5, 12),
            24.90m,
            "FARMACIA SAO JOSE",
            new DateOnly(2026, 5, 12),
            29.90m);

        var matches = reasons ?? throw new InvalidOperationException(
            "Same-day normalized description should suggest a candidate with an amount difference.");
        TestAssert.True(matches.Contains("Valor diferente"), "The amount difference should be visible for review");
        return Task.CompletedTask;
    }

    public static Task UnrelatedManualExpenseIsNotSuggested()
    {
        var reasons = Match(
            "Aluguel",
            new DateOnly(2026, 5, 15),
            1200m,
            "Cinema",
            new DateOnly(2026, 5, 12),
            42.50m);

        TestAssert.True(reasons == null, "Unrelated date, amount and description should not suggest a match");
        return Task.CompletedTask;
    }

    private static IReadOnlyList<string>? Match(
        string manualDescription,
        DateOnly manualDate,
        decimal manualAmount,
        string sourceDescription,
        DateOnly sourceDate,
        decimal sourceAmount) => MatchMethod.Invoke(null, new object?[]
        {
            manualDescription,
            manualDate,
            manualAmount,
            sourceDescription,
            sourceDate,
            sourceAmount
        }) as IReadOnlyList<string>;
}
