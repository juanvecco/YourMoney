using Microsoft.AspNetCore.Identity;

namespace YourMoney.Api.Services;

public enum ExternalAccountStatus { Linked, New, LoginNotLinked, EmailAlreadyRegistered }
public sealed record ExternalAccountResolution(ExternalAccountStatus Status, IdentityUser? User = null);

public static class ExternalAccountResolver
{
    public static async Task<ExternalAccountResolution> ResolveAsync(
        UserManager<IdentityUser> users, string provider, string providerKey, string? email, bool loginOnly)
    {
        var linked = await users.FindByLoginAsync(provider, providerKey);
        if (linked != null) return new ExternalAccountResolution(ExternalAccountStatus.Linked, linked);
        if (loginOnly) return new ExternalAccountResolution(ExternalAccountStatus.LoginNotLinked);
        if (!string.IsNullOrWhiteSpace(email) && await users.FindByEmailAsync(email) != null)
            return new ExternalAccountResolution(ExternalAccountStatus.EmailAlreadyRegistered);
        return new ExternalAccountResolution(ExternalAccountStatus.New);
    }
}
