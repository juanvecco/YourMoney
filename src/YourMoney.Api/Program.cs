using YourMoney.Api.Configuration;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using YourMoney.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
DeploymentConfig.Validate(builder.Configuration, builder.Environment);

if (int.TryParse(builder.Configuration["PORT"], out var port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var dataProtection = builder.Services
    .AddDataProtection()
    .SetApplicationName("YourMoney");

if (!builder.Environment.IsDevelopment())
{
    var certificate = X509CertificateLoader.LoadPkcs12FromFile(
        builder.Configuration["DATA_PROTECTION_CERTIFICATE_PATH"]!,
        builder.Configuration["DATA_PROTECTION_CERTIFICATE_PASSWORD"]);
    dataProtection.PersistKeysToDbContext<ApplicationDbContext>()
        .ProtectKeysWithCertificate(certificate);
}
else
{
    var configuredKeyPath = builder.Configuration["DATA_PROTECTION_KEYS_PATH"]
        ?? builder.Configuration["DataProtection:KeysPath"];
    var keyPath = string.IsNullOrWhiteSpace(configuredKeyPath)
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YourMoney", "DataProtection-Keys")
        : Path.GetFullPath(configuredKeyPath);
    Directory.CreateDirectory(keyPath);
    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        File.SetUnixFileMode(keyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keyPath));
    if (OperatingSystem.IsWindows())
        dataProtection.ProtectKeysWithDpapi();
}

builder.Services
    .AddIdentityConfiguration(builder.Configuration)
    .AddDatabaseConfiguration(builder.Configuration)
    .AddDependencyInjectionConfiguration()
    .AddApiConfiguration(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerConfiguration();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseApiConfiguration(app.Environment);

app.Run();
