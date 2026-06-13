using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class PublishedAngularAppTests
    {
        public static Task PublishedEntryPointIsAngularAppWithBearerInterceptor()
        {
            var wwwrootPath = Path.Combine(
                FindSolutionRoot(),
                "src",
                "YourMoney.Api",
                "wwwroot");

            var indexPath = Path.Combine(wwwrootPath, "index.html");
            var indexHtml = File.ReadAllText(indexPath);
            var mainBundlePath = Directory
                .EnumerateFiles(wwwrootPath, "main-*.js", SearchOption.TopDirectoryOnly)
                .Single();
            var mainBundle = File.ReadAllText(mainBundlePath);

            TestAssert.True(indexHtml.Contains("<app-root>", StringComparison.Ordinal), "Published index.html should be the Angular entry point");
            TestAssert.True(indexHtml.Contains("main-", StringComparison.Ordinal), "Published index.html should reference the Angular main bundle");
            TestAssert.True(mainBundle.Contains("Authorization", StringComparison.Ordinal), "Published bundle should include Authorization header logic");
            TestAssert.True(mainBundle.Contains("Bearer", StringComparison.Ordinal), "Published bundle should attach bearer tokens");
            TestAssert.True(mainBundle.Contains("access_token", StringComparison.Ordinal), "Published bundle should read the stored access token");
            TestAssert.True(mainBundle.Contains("https://localhost:5001/api", StringComparison.Ordinal), "Published bundle should call the local API base URL used by the hosted frontend");
            TestAssert.True(!File.Exists(Path.Combine(wwwrootPath, "script.json")), "Legacy static script should not be published");
            TestAssert.True(!File.Exists(Path.Combine(wwwrootPath, "styles.css")), "Legacy static stylesheet should not be published");
            TestAssert.True(!indexHtml.Contains("fetch('/api/transactions')", StringComparison.Ordinal), "Legacy unauthenticated transaction fetch should not be published");

            return Task.CompletedTask;
        }

        private static string FindSolutionRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "src", "YourMoney.Api")))
                    return current.FullName;

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate YourMoney solution root.");
        }
    }
}
