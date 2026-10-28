using YourMoney.Api.Configuration;

using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var configuredKeyPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_PATH")
    ?? builder.Configuration["DataProtection:KeysPath"];
var keyPath = string.IsNullOrWhiteSpace(configuredKeyPath)
    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YourMoney", "DataProtection-Keys")
    : Path.GetFullPath(configuredKeyPath);
if (string.IsNullOrWhiteSpace(keyPath))
    throw new InvalidOperationException("Configure um diretório persistente para as chaves de proteção de dados.");

Directory.CreateDirectory(keyPath);
if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
{
    File.SetUnixFileMode(
        keyPath,
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
}

var dataProtection = builder.Services
    .AddDataProtection()
    .SetApplicationName("YourMoney")
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath));
if (OperatingSystem.IsWindows())
    dataProtection.ProtectKeysWithDpapi();

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
