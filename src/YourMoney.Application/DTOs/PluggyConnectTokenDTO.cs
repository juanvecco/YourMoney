namespace YourMoney.Application.DTOs;

public sealed class PluggyConnectTokenRequestDTO
{
    public Guid? ConnectionId { get; set; }
}

public sealed class PluggyConnectTokenResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public Guid? ConnectionId { get; set; }
}
