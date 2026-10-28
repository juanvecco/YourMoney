using System.Net;
using System.Net.Mail;

namespace YourMoney.Api.Services;

public sealed class SmtpPasswordResetDeliveryService : IPasswordResetDeliveryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpPasswordResetDeliveryService> _logger;

    public SmtpPasswordResetDeliveryService(
        IConfiguration configuration,
        ILogger<SmtpPasswordResetDeliveryService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task DeliverAsync(string email, string resetUrl, CancellationToken cancellationToken = default)
    {
        var host = Read("SMTP_HOST", "PasswordReset:Smtp:Host");
        var sender = Read("SMTP_FROM", "PasswordReset:Smtp:From");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(sender))
            throw new InvalidOperationException("A entrega de recuperação de senha não está configurada.");

        var port = int.TryParse(Read("SMTP_PORT", "PasswordReset:Smtp:Port"), out var configuredPort)
            ? configuredPort : 587;
        var username = Read("SMTP_USERNAME", "PasswordReset:Smtp:Username");
        var password = Read("SMTP_PASSWORD", "PasswordReset:Smtp:Password");
        var enableSsl = !bool.TryParse(Read("SMTP_SSL", "PasswordReset:Smtp:EnableSsl"), out var configuredSsl)
            || configuredSsl;

        using var message = new MailMessage(sender, email)
        {
            Subject = "Redefina sua senha do YourMoney",
            Body = "Recebemos uma solicitação para redefinir sua senha.\n\n"
                + resetUrl
                + "\n\nSe você não fez esta solicitação, ignore esta mensagem.",
            IsBodyHtml = false
        };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = string.IsNullOrWhiteSpace(username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(username, password)
        };

        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch
        {
            _logger.LogError("Falha ao enviar uma mensagem de recuperação de senha pelo provedor configurado.");
            throw;
        }
    }

    private string? Read(string environmentName, string configurationKey) =>
        Environment.GetEnvironmentVariable(environmentName) ?? _configuration[configurationKey];
}
