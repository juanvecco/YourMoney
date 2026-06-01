using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class StaticApplicationHostingTests
    {
        public static Task ApiConfigurationServesAngularSpaWithRouteFallback()
        {
            var apiConfigPath = Path.Combine(
                FindSolutionRoot(),
                "src",
                "YourMoney.Api",
                "Configuration",
                "ApiConfig.cs");

            var source = File.ReadAllText(apiConfigPath);
            var swaggerConfigPath = Path.Combine(
                FindSolutionRoot(),
                "src",
                "YourMoney.Api",
                "Configuration",
                "SwaggerConfig.cs");
            var swaggerSource = File.ReadAllText(swaggerConfigPath);

            var defaultFilesIndex = source.IndexOf("UseDefaultFiles", StringComparison.Ordinal);
            var staticFilesIndex = source.IndexOf("UseStaticFiles", StringComparison.Ordinal);
            var routingIndex = source.IndexOf("UseRouting", StringComparison.Ordinal);
            var fallbackIndex = source.IndexOf("MapFallbackToFile(\"index.html\")", StringComparison.Ordinal);

            TestAssert.True(defaultFilesIndex >= 0, "API should enable default files for the Angular entry point");
            TestAssert.True(staticFilesIndex > defaultFilesIndex, "API should serve static files after default file resolution");
            TestAssert.True(routingIndex > staticFilesIndex, "API should serve static files before endpoint routing");
            TestAssert.True(fallbackIndex > routingIndex, "API should map SPA routes back to index.html");
            TestAssert.True(swaggerSource.Contains("RoutePrefix = \"swagger\"", StringComparison.Ordinal), "Swagger UI should not capture / or /index.html");

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
