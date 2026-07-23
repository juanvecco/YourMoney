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

        public static Task StartupScriptPublishesAngularBuildBeforeOpeningHostedFrontend()
        {
            var scriptPath = Path.Combine(FindRepositoryRoot(), "abrir-yourmoney.ps1");
            var source = File.ReadAllText(scriptPath);

            var publishCallIndex = source.IndexOf("Publish-AngularToApi", StringComparison.Ordinal);
            var updateDatabaseCallIndex = source.IndexOf("Update-Database", publishCallIndex + 1, StringComparison.Ordinal);
            var startApiCallIndex = source.IndexOf("Start-Api", publishCallIndex + 1, StringComparison.Ordinal);
            var openFrontendIndex = source.IndexOf("Start-Process \"https://localhost:5001/dashboard\"", StringComparison.Ordinal);

            TestAssert.True(source.Contains("& npm.cmd run build", StringComparison.Ordinal), "Startup script should build the Angular app before opening the hosted frontend");
            TestAssert.True(source.Contains("dist\\yourmoney-app\\browser", StringComparison.Ordinal), "Startup script should use the Angular browser build output");
            TestAssert.True(source.Contains("src\\YourMoney.Api\\wwwroot", StringComparison.Ordinal), "Startup script should publish the Angular build into the API wwwroot");
            TestAssert.True(source.Contains("Copy-Item", StringComparison.Ordinal), "Startup script should copy the fresh Angular build into the hosted static files");
            TestAssert.True(source.Contains("dotnet ef database update", StringComparison.Ordinal), "Startup script should apply pending EF migrations before opening the hosted frontend");
            TestAssert.True(publishCallIndex >= 0 && publishCallIndex < startApiCallIndex, "Startup script should publish the Angular build before starting the API");
            TestAssert.True(updateDatabaseCallIndex >= 0 && publishCallIndex < updateDatabaseCallIndex, "Startup script should apply database migrations after publishing the Angular build");
            TestAssert.True(updateDatabaseCallIndex >= 0 && updateDatabaseCallIndex < startApiCallIndex, "Startup script should apply database migrations before starting or reusing the API");
            TestAssert.True(startApiCallIndex >= 0 && startApiCallIndex < openFrontendIndex, "Startup script should start the API before opening the hosted frontend");

            return Task.CompletedTask;
        }

        public static Task StartupScriptStopsOnlyProjectApiBeforeBuildAndMigration()
        {
            var scriptPath = Path.Combine(FindRepositoryRoot(), "abrir-yourmoney.ps1");
            var source = File.ReadAllText(scriptPath);
            var stopCallIndex = source.LastIndexOf("Stop-ExistingApi", StringComparison.Ordinal);
            var publishCallIndex = source.LastIndexOf("Publish-AngularToApi", StringComparison.Ordinal);
            var updateDatabaseCallIndex = source.LastIndexOf("Update-Database", StringComparison.Ordinal);

            TestAssert.True(source.Contains("Get-Process -Name \"YourMoney.Api\"", StringComparison.Ordinal), "Startup script should target only YourMoney.Api processes");
            TestAssert.True(source.Contains("resolvedProcessPath.StartsWith($apiExecutableRoot", StringComparison.Ordinal), "Startup script should verify the API executable belongs to this project");
            TestAssert.True(source.Contains("Stop-Process -Id $process.Id", StringComparison.Ordinal), "Startup script should release locked API assemblies");
            TestAssert.True(stopCallIndex >= 0 && stopCallIndex < publishCallIndex, "Startup script should stop the old API before publishing Angular files");
            TestAssert.True(stopCallIndex < updateDatabaseCallIndex, "Startup script should stop the old API before EF builds and applies migrations");

            return Task.CompletedTask;
        }

        public static Task StartupCmdDelegatesToPowerShellScript()
        {
            var commandPath = Path.Combine(FindRepositoryRoot(), "abrir-yourmoney.cmd");
            var source = File.ReadAllText(commandPath);

            TestAssert.True(source.Contains("abrir-yourmoney.ps1", StringComparison.Ordinal), "CMD startup should delegate to the PowerShell startup script");
            TestAssert.True(source.Contains("cd /d \"%SCRIPT_DIR%\"", StringComparison.Ordinal), "CMD startup should execute from the repository root even when launched elsewhere");
            TestAssert.True(source.Contains("pwsh.exe", StringComparison.Ordinal), "CMD startup should prefer PowerShell 7 when available");
            TestAssert.True(source.Contains("powershell.exe", StringComparison.Ordinal), "CMD startup should fall back to Windows PowerShell");
            TestAssert.True(source.Contains("exit /b %EXIT_CODE%", StringComparison.Ordinal), "CMD startup should preserve the PowerShell script exit code");

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
