using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class PasswordRecoveryControllerTests
{
    public static Task RecoveryIsNeutralDeliveredAndSecretFree()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Controllers", "AuthController.cs"));
        TestAssert.True(source.Contains("_passwordResetDelivery.DeliverAsync"), "Existing users should receive the reset link through the delivery abstraction");
        TestAssert.True(source.Contains("Se o e-mail estiver cadastrado"), "The public response must not enumerate accounts");
        TestAssert.True(!source.Contains("return CustomResponse(new { token"), "The reset token must never be returned");
        TestAssert.True(source.Contains("Não foi possível redefinir a senha"), "Invalid reset tokens should return a normalized error");

        var delivery = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "SmtpPasswordResetDeliveryService.cs"));
        TestAssert.True(!delivery.Contains("LogInformation(resetUrl"), "Reset links must not be logged");
        return Task.CompletedTask;
    }
}
