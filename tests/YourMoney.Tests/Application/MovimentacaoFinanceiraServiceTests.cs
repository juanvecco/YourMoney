using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class MovimentacaoFinanceiraServiceTests
{
    public static Task ProjectionIsOwnerScopedAndTotalsPrecedePagination()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Application", "Services", "MovimentacaoFinanceiraService.cs"));
        TestAssert.True(source.Contains("_currentUser.UserId"), "Activity must use the authenticated owner");
        TestAssert.True(source.Contains("Task.WhenAll"), "Independent projections should execute in parallel");
        TestAssert.True(source.IndexOf("new ResumoFinanceiroPeriodoDto", StringComparison.Ordinal) <
                        source.IndexOf(".Skip(", StringComparison.Ordinal),
            "Totals must be computed before pagination");
        TestAssert.True(source.Contains("ThenBy(item => item.Id)"), "Ordering must have a stable id tie-breaker");
        return Task.CompletedTask;
    }

    public static Task QueryBoundsAreValidated()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Application", "Services", "MovimentacaoFinanceiraService.cs"));
        TestAssert.True(source.Contains("AddMonths(12)"), "Activity range must be bounded to twelve months");
        TestAssert.True(source.Contains("TamanhoPagina is < 10 or > 100"), "Page size must be bounded");
        TestAssert.True(source.Contains("Busca?.Trim().Length > 100"), "Search input must be bounded");
        return Task.CompletedTask;
    }
}
