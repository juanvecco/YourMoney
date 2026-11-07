namespace YourMoney.Domain.Entities;

public sealed class PluggyCredential : BaseEntity
{
    public Guid CarteiraId { get; private set; }
    public string ClientId { get; private set; } = string.Empty;
    public string ProtectedClientSecret { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; private set; }

    private PluggyCredential() { }

    public PluggyCredential(string usuarioId, string clientId, string protectedClientSecret)
    {
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        Atualizar(clientId, protectedClientSecret);
    }

    public void Atualizar(string clientId, string protectedClientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("O Client ID da Pluggy é obrigatório.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(protectedClientSecret))
            throw new ArgumentException("O segredo protegido da Pluggy é obrigatório.", nameof(protectedClientSecret));

        ClientId = clientId.Trim();
        ProtectedClientSecret = protectedClientSecret;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AssociarCarteira(Guid carteiraId)
    {
        if (carteiraId == Guid.Empty)
            throw new ArgumentException("A carteira é obrigatória.", nameof(carteiraId));
        CarteiraId = carteiraId;
    }
}
