using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class StaticApplicationHostingTests
    {
        public static Task StartupScriptOpensHostedDashboardAsPrimaryFrontendUrl()
        {
            var scriptPath = Path.Combine(FindRepositoryRoot(), "abrir-yourmoney.ps1");
            var source = File.ReadAllText(scriptPath);

            TestAssert.True(
                source.Contains("Start-Process \"https://localhost:5001/dashboard\"", StringComparison.Ordinal),
                "Startup script should open the hosted Angular dashboard as the primary frontend URL");
            TestAssert.True(
                !source.Contains("Start-Process \"http://127.0.0.1:4200/dashboard\"", StringComparison.Ordinal),
                "Startup script should not open the Angular dev server as the primary user-facing frontend URL");

            return Task.CompletedTask;
        }

        public static Task StartupScriptPrintsTheSameFrontendUrlItOpens()
        {
            var scriptPath = Path.Combine(FindRepositoryRoot(), "abrir-yourmoney.ps1");
            var source = File.ReadAllText(scriptPath);

            TestAssert.True(
                source.Contains("Start-Process \"https://localhost:5001/dashboard\"", StringComparison.Ordinal),
                "Startup script should open the hosted dashboard URL");
            TestAssert.True(
                source.Contains("Write-Host \"- Frontend: https://localhost:5001/dashboard\"", StringComparison.Ordinal),
                "Startup script should print the same hosted dashboard URL as the frontend URL");
            TestAssert.True(
                source.Contains("Write-Host \"- API Swagger: https://localhost:5001/swagger\"", StringComparison.Ordinal),
                "Startup script should keep Swagger listed as an API support URL");

            return Task.CompletedTask;
        }

        public static Task CorsAllowsLocalDevelopmentOriginsWithoutCredentialSharing()
        {
            var apiConfigPath = Path.Combine(
                FindSolutionRoot(),
                "src",
                "YourMoney.Api",
                "Configuration",
                "ApiConfig.cs");

            var source = File.ReadAllText(apiConfigPath);

            TestAssert.True(source.Contains("AllowAnyOrigin", StringComparison.Ordinal), "API should accept local development frontend origins");
            TestAssert.True(source.Contains("AllowAnyHeader", StringComparison.Ordinal), "API should accept Authorization headers from local development origins");
            TestAssert.True(source.Contains("AllowAnyMethod", StringComparison.Ordinal), "API should accept protected REST methods from local development origins");
            TestAssert.True(!source.Contains("AllowCredentials", StringComparison.Ordinal), "API should not enable credential sharing for bearer-token local origins");

            return Task.CompletedTask;
        }

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

        private static string FindRepositoryRoot()
        {
            var solutionRoot = FindSolutionRoot();
            var repositoryRoot = Directory.GetParent(solutionRoot)?.FullName;

            if (repositoryRoot != null && File.Exists(Path.Combine(repositoryRoot, "abrir-yourmoney.ps1")))
                return repositoryRoot;

            throw new DirectoryNotFoundException("Could not locate YourMoney repository root.");
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
