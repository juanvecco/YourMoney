namespace YourMoney.Api.Configuration;

public static class DeploymentConfig
{
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        var port = configuration["PORT"];
        if (!string.IsNullOrWhiteSpace(port)
            && (!int.TryParse(port, out var number) || number is < 1 or > 65535))
            throw new InvalidOperationException("PORT deve ser um número entre 1 e 65535.");

        if (environment.IsDevelopment())
            return;

        if (!string.Equals(configuration["DATABASE_PROVIDER"], "PostgreSQL", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(configuration["DATABASE_PROVIDER"], "Postgres", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("DATABASE_PROVIDER deve ser PostgreSQL em hospedagem.");

        if (string.IsNullOrWhiteSpace(configuration["DATABASE_CONNECTION_STRING"]))
            throw new InvalidOperationException("DATABASE_CONNECTION_STRING é obrigatória em hospedagem.");

        if (string.IsNullOrWhiteSpace(configuration["JwtSettings:Secret"])
            || configuration["JwtSettings:Secret"]!.Length < 32)
            throw new InvalidOperationException("JwtSettings__Secret deve ter pelo menos 32 caracteres.");

        var certificatePath = configuration["DATA_PROTECTION_CERTIFICATE_PATH"];
        var certificateBase64 = configuration["DATA_PROTECTION_CERTIFICATE_BASE64"];
        if (string.IsNullOrWhiteSpace(certificateBase64)
            && (string.IsNullOrWhiteSpace(certificatePath) || !File.Exists(certificatePath)))
            throw new InvalidOperationException("Configure DATA_PROTECTION_CERTIFICATE_BASE64 ou DATA_PROTECTION_CERTIFICATE_PATH com um certificado existente.");

        if (string.IsNullOrWhiteSpace(configuration["DATA_PROTECTION_CERTIFICATE_PASSWORD"]))
            throw new InvalidOperationException("DATA_PROTECTION_CERTIFICATE_PASSWORD é obrigatória.");
    }
}
