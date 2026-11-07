using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using YourMoney.Api.Configuration;
using YourMoney.Infrastructure.Data;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class DeploymentConfigTests
{
    public static Task ProductionRejectsMissingSecretsWithoutEchoingValues()
    {
        const string marker = "PRIVATE_CONNECTION_MARKER";
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DATABASE_PROVIDER"] = "PostgreSQL",
            ["DATABASE_CONNECTION_STRING"] = marker,
            ["JwtSettings:Secret"] = "short"
        }).Build();
        try
        {
            DeploymentConfig.Validate(config, new TestEnvironment("Production"));
            throw new InvalidOperationException("A weak JWT secret was accepted.");
        }
        catch (InvalidOperationException ex)
        {
            TestAssert.True(!ex.Message.Contains(marker), "Configuration error leaked connection material");
            TestAssert.True(ex.Message.Contains("JwtSettings__Secret"), "Missing JWT error should be actionable");
        }
        return Task.CompletedTask;
    }

    public static Task ProductionAcceptsExternalConfigAndPersistsKeysInDbContext()
    {
        var certificatePath = Path.GetTempFileName();
        try
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATABASE_PROVIDER"] = "PostgreSQL",
                ["DATABASE_CONNECTION_STRING"] = "Host=example.invalid;Database=test;",
                ["JwtSettings:Secret"] = new string('x', 32),
                ["DATA_PROTECTION_CERTIFICATE_PATH"] = certificatePath,
                ["DATA_PROTECTION_CERTIFICATE_PASSWORD"] = "test-password",
                ["PORT"] = "10000"
            }).Build();
            DeploymentConfig.Validate(config, new TestEnvironment("Production"));
            TestAssert.True(typeof(IDataProtectionKeyContext).IsAssignableFrom(typeof(ApplicationDbContext)),
                "Identity context must persist keys outside the container");
        }
        finally { File.Delete(certificatePath); }
        return Task.CompletedTask;
    }

    public static Task ProductionAcceptsBase64CertificateConfig()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DATABASE_PROVIDER"] = "PostgreSQL",
            ["DATABASE_CONNECTION_STRING"] = "Host=example.invalid;Database=test;",
            ["JwtSettings:Secret"] = new string('x', 32),
            ["DATA_PROTECTION_CERTIFICATE_BASE64"] = "AQID",
            ["DATA_PROTECTION_CERTIFICATE_PASSWORD"] = "test-password"
        }).Build();
        DeploymentConfig.Validate(config, new TestEnvironment("Production"));
        return Task.CompletedTask;
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "YourMoney.Tests";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
