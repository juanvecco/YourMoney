using Microsoft.AspNetCore.Authorization;
using YourMoney.Api.Controllers;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class MovimentacoesControllerTests
{
    public static Task EndpointIsProtectedAndMapsStableErrors()
    {
        TestAssert.True(typeof(MovimentacoesController).IsDefined(typeof(AuthorizeAttribute), true),
            "Activity endpoint must require authentication");
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Controllers", "MovimentacoesController.cs"));
        TestAssert.True(source.Contains("Status400BadRequest"), "Invalid queries must map to 400");
        TestAssert.True(source.Contains("Status500InternalServerError"), "Unexpected failures must map to 500");
        TestAssert.True(!source.Contains("ex.ToString()"), "API errors must not leak stack traces");
        return Task.CompletedTask;
    }
}
