namespace YourMoney.Domain.Entities;

public sealed class PluggyAccountLink : BaseEntity
{
    public Guid ConnectionId { get; private set; }
    public string AccountId { get; private set; } = string.Empty;
    public Guid ContaFinanceiraId { get; private set; }

    private PluggyAccountLink() { }

    public PluggyAccountLink(string usuarioId, Guid connectionId, string accountId, Guid contaFinanceiraId)
    {
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        ConnectionId = connectionId;
        AccountId = accountId;
        ContaFinanceiraId = contaFinanceiraId;
    }

    public void Vincular(Guid contaFinanceiraId) => ContaFinanceiraId = contaFinanceiraId;
}
