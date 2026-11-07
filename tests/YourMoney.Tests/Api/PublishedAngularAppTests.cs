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
            var mainBundleReference = System.Text.RegularExpressions.Regex.Match(
                indexHtml,
                "(?:src|href)=\"(?<bundle>main-[^\"]+\\.js)\"",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant);
            TestAssert.True(mainBundleReference.Success, "Published index.html should reference exactly one Angular main bundle");
            var mainBundlePath = Path.Combine(wwwrootPath, mainBundleReference.Groups["bundle"].Value);
            var mainBundle = File.ReadAllText(mainBundlePath);
            var activeScripts = new List<string>();
            var pending = new Queue<string>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            pending.Enqueue(mainBundleReference.Groups["bundle"].Value);
            while (pending.Count > 0)
            {
                var name = pending.Dequeue();
                if (!visited.Add(name)) continue;
                var content = File.ReadAllText(Path.Combine(wwwrootPath, name));
                activeScripts.Add(content);
                foreach (System.Text.RegularExpressions.Match chunk in
                    System.Text.RegularExpressions.Regex.Matches(content, "chunk-[A-Za-z0-9]+\\.js"))
                {
                    if (File.Exists(Path.Combine(wwwrootPath, chunk.Value)))
                        pending.Enqueue(chunk.Value);
                }
            }
            var publishedCode = string.Join("\n", activeScripts);

            TestAssert.True(indexHtml.Contains("<app-root>", StringComparison.Ordinal), "Published index.html should be the Angular entry point");
            TestAssert.True(indexHtml.Contains("main-", StringComparison.Ordinal), "Published index.html should reference the Angular main bundle");
            TestAssert.True(mainBundle.Contains("Authorization", StringComparison.Ordinal), "Published bundle should include Authorization header logic");
            TestAssert.True(mainBundle.Contains("Bearer", StringComparison.Ordinal), "Published bundle should attach bearer tokens");
            TestAssert.True(mainBundle.Contains("access_token", StringComparison.Ordinal), "Published bundle should read the stored access token");
            var usesLocalApiBase = publishedCode.Contains("https://localhost:5001/api", StringComparison.Ordinal)
                || publishedCode.Contains("apiUrl:\"/api\"", StringComparison.Ordinal);
            TestAssert.True(usesLocalApiBase, "Published bundle should use the local API base URL or the same-origin /api route");
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
