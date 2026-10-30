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
    public DateTimeOffset? InvestmentAssetsLastAttemptAt { get; private set; }
    public DateTimeOffset? InvestmentAssetsLastSuccessAt { get; private set; }
    public string InvestmentAssetsSyncStatus { get; private set; } = "never";
    public string? InvestmentAssetsSyncMessage { get; private set; }
    public bool? HasInvestmentProduct { get; private set; }
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

    public void RegistrarTentativaAtivos(DateTimeOffset attemptedAt)
    {
        InvestmentAssetsLastAttemptAt = attemptedAt;
        InvestmentAssetsSyncStatus = "loading";
        InvestmentAssetsSyncMessage = null;
    }

    public void RegistrarSucessoAtivos(DateTimeOffset succeededAt, bool hasInvestmentProduct)
    {
        InvestmentAssetsLastAttemptAt = succeededAt;
        InvestmentAssetsLastSuccessAt = succeededAt;
        HasInvestmentProduct = hasInvestmentProduct;
        InvestmentAssetsSyncStatus = hasInvestmentProduct ? "current" : "no_product";
        InvestmentAssetsSyncMessage = null;
    }

    public void RegistrarFalhaAtivos(DateTimeOffset attemptedAt, string message)
    {
        InvestmentAssetsLastAttemptAt = attemptedAt;
        InvestmentAssetsSyncStatus = "stale";
        var normalized = string.IsNullOrWhiteSpace(message)
            ? "Não foi possível atualizar os ativos desta instituição."
            : message.Trim();
        InvestmentAssetsSyncMessage = normalized[..Math.Min(normalized.Length, 500)];
    }

    public void RegistrarErroConfiguracaoAtivos(string message)
    {
        InvestmentAssetsSyncStatus = "configuration_required";
        InvestmentAssetsSyncMessage = string.IsNullOrWhiteSpace(message)
            ? null : message.Trim()[..Math.Min(message.Trim().Length, 500)];
    }
}
