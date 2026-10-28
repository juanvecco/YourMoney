namespace YourMoney.Api.Services;

public interface IPasswordResetDeliveryService
{
    Task DeliverAsync(string email, string resetUrl, CancellationToken cancellationToken = default);
}
