using System.ComponentModel.DataAnnotations;

namespace YourMoney.Api.Models;

public sealed record ExternalAccountLinkStatus(bool GoogleAvailable, bool GoogleLinked);

public sealed class PrepareGoogleLinkRequest
{
    [Required]
    public string SenhaAtual { get; set; } = "";
}

public sealed record PrepareGoogleLinkResponse(string ContinuationPath);

public sealed record PendingGoogleLinkResponse(string Provider, string EmailMasked, DateTimeOffset ExpiresAt);

public sealed record GoogleLinkStartTicket(string UserId, string Nonce);

public sealed record GoogleLinkPendingTicket(string UserId, string ProviderKey, string Email, string Nonce);

public enum ExternalAccountLinkFailure
{
    None,
    AccountNotFound,
    InvalidPassword,
    AccountLocked,
    AlreadyLinked,
    InvalidTicket,
    EmailMismatch,
    IdentityConflict
}

public sealed record ExternalAccountLinkResult(
    bool Succeeded,
    ExternalAccountLinkFailure Failure = ExternalAccountLinkFailure.None,
    string? Ticket = null,
    DateTimeOffset? ExpiresAt = null,
    ExternalAccountLinkStatus? Status = null);
