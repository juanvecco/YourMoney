using Microsoft.AspNetCore.Authorization;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class ContaFinanceiraControllerTests
{
    public static Task CrudIsProtectedAndTyped()
    {
        TestAssert.True(typeof(ContaFinanceiraController).IsDefined(typeof(AuthorizeAttribute), true),
            "Account endpoints must require authentication");
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Controllers", "ContaFinanceiraController.cs"));
        TestAssert.True(source.Contains("CreatedAtAction"), "Create should return 201 with a typed resource");
        TestAssert.True(source.Contains("Status409Conflict"), "Linked deletion should return 409");
        TestAssert.True(source.Contains("NoContent()"), "Successful deletion should return 204");
        return Task.CompletedTask;
    }
}
