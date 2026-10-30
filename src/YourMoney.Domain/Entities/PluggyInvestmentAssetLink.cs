namespace YourMoney.Domain.Entities;

public sealed class PluggyInvestmentAssetLink : BaseEntity
{
    public Guid ConnectionId { get; private set; }
    public string PluggyItemId { get; private set; } = string.Empty;
    public string ExternalInvestmentId { get; private set; } = string.Empty;
    public Guid InvestimentoId { get; private set; }
    public string Action { get; private set; } = "link";
    public DateTimeOffset RegisteredAt { get; private set; }

    private PluggyInvestmentAssetLink() { }

    public PluggyInvestmentAssetLink(string usuarioId, Guid connectionId, string itemId,
        string externalInvestmentId, Guid investimentoId, string action, DateTimeOffset registeredAt)
    {
        if (connectionId == Guid.Empty || investimentoId == Guid.Empty
            || string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(externalInvestmentId)
            || action is not ("link" or "create" or "replace"))
            throw new ArgumentException("Os dados do vínculo da posição são inválidos.");
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        ConnectionId = connectionId;
        PluggyItemId = itemId.Trim();
        ExternalInvestmentId = externalInvestmentId.Trim();
        InvestimentoId = investimentoId;
        Action = action;
        RegisteredAt = registeredAt;
    }
}
