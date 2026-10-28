namespace YourMoney.Domain.Entities;

public sealed class PluggyConnection : BaseEntity
{
    public string PluggyItemId { get; private set; } = string.Empty;
    public string InstitutionName { get; private set; } = string.Empty;
    public int ConnectorId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public bool IsSandbox { get; private set; }
    public DateTimeOffset? LastUpdatedAt { get; private set; }
    public DateTimeOffset? ConsentExpiresAt { get; private set; }
    public DateTimeOffset ConnectedAt { get; private set; }

    private PluggyConnection() { }

    public PluggyConnection(
        string usuarioId,
        string pluggyItemId,
        string institutionName,
        int connectorId,
        string status,
        bool isSandbox,
        DateTimeOffset? lastUpdatedAt,
        DateTimeOffset? consentExpiresAt = null)
    {
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        Atualizar(pluggyItemId, institutionName, connectorId, status, isSandbox, lastUpdatedAt, consentExpiresAt);
        ConnectedAt = DateTimeOffset.UtcNow;
    }

    public void Atualizar(
        string pluggyItemId,
        string institutionName,
        int connectorId,
        string status,
        bool isSandbox,
        DateTimeOffset? lastUpdatedAt,
        DateTimeOffset? consentExpiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(pluggyItemId))
            throw new ArgumentException("O identificador da conexão é obrigatório.", nameof(pluggyItemId));
        if (string.IsNullOrWhiteSpace(institutionName))
            throw new ArgumentException("O nome da instituição é obrigatório.", nameof(institutionName));

        PluggyItemId = pluggyItemId.Trim();
        InstitutionName = institutionName.Trim();
        ConnectorId = connectorId;
        Status = string.IsNullOrWhiteSpace(status) ? "UPDATED" : status.Trim();
        IsSandbox = isSandbox;
        LastUpdatedAt = lastUpdatedAt;
        ConsentExpiresAt = consentExpiresAt;
    }
}
