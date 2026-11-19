namespace YourMoney.Api.Configuration;

public sealed record ExternalProviderSettings(
    string ClientId,
    string ClientSecret,
    string TeamId = "",
    string KeyId = "",
    string PrivateKeyBase64 = "");

public static class ExternalProviderConfiguration
{
    public static ExternalProviderSettings Google(IConfiguration configuration) => new(
        Read(configuration, "GOOGLE_CLIENT_ID", "ExternalAuth:Google:ClientId"),
        Read(configuration, "GOOGLE_CLIENT_SECRET", "ExternalAuth:Google:ClientSecret"));

    public static ExternalProviderSettings Apple(IConfiguration configuration) => new(
        Read(configuration, "APPLE_SERVICE_ID", "ExternalAuth:Apple:ServiceId"),
        "",
        Read(configuration, "APPLE_TEAM_ID", "ExternalAuth:Apple:TeamId"),
        Read(configuration, "APPLE_KEY_ID", "ExternalAuth:Apple:KeyId"),
        Read(configuration, "APPLE_PRIVATE_KEY_BASE64", "ExternalAuth:Apple:PrivateKeyBase64"));

    public static bool GoogleAvailable(IConfiguration configuration)
    {
        var settings = Google(configuration);
        return !string.IsNullOrWhiteSpace(settings.ClientId)
            && !string.IsNullOrWhiteSpace(settings.ClientSecret);
    }

    public static bool AppleAvailable(IConfiguration configuration)
    {
        var settings = Apple(configuration);
        return !string.IsNullOrWhiteSpace(settings.ClientId)
            && !string.IsNullOrWhiteSpace(settings.TeamId)
            && !string.IsNullOrWhiteSpace(settings.KeyId)
            && !string.IsNullOrWhiteSpace(settings.PrivateKeyBase64);
    }

    private static string Read(IConfiguration configuration, string environmentKey, string configKey) =>
        Environment.GetEnvironmentVariable(environmentKey) ?? configuration[configKey] ?? "";
}
