using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class ContaFinanceiraServiceTests
{
    public static Task CrudUsesAuthenticatedOwnerAndConflictDetection()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Application", "Services", "ContaFinanceiraService.cs"));
        TestAssert.True(source.Contains("_currentUserService.UserId"), "Account CRUD must use the authenticated owner");
        TestAssert.True(source.Contains("TemVinculosAsync"), "Account deletion must check linked records");
        TestAssert.True(source.Contains("ContaFinanceiraEmUsoException"), "Linked account deletion must use a typed conflict");
        TestAssert.True(!source.Contains("YourMoney.Infrastructure"), "Application must not import Infrastructure");
        return Task.CompletedTask;
    }
}
