namespace YourMoney.Application.DTOs;

public sealed class PluggyCredentialRequestDTO
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public sealed class PluggyCredentialStatusDTO
{
    public bool Configured { get; set; }
    public string Source { get; set; } = "none";
    public string? ClientIdHint { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool CanSave { get; set; }
    public string? Message { get; set; }
}
