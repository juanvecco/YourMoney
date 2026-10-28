namespace YourMoney.Domain.Entities;

public sealed class PluggyTransactionImport : BaseEntity
{
    public string TransactionId { get; private set; } = string.Empty;
    public Guid ConnectionId { get; private set; }
    public string RecordType { get; private set; } = string.Empty;
    public Guid RecordId { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }

    private PluggyTransactionImport()
    {
    }

    public PluggyTransactionImport(
        string usuarioId,
        string transactionId,
        Guid connectionId,
        string recordType,
        Guid recordId,
        DateTimeOffset registeredAt)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("O identificador da transação Pluggy é obrigatório.", nameof(transactionId));
        if (connectionId == Guid.Empty)
            throw new ArgumentException("A conexão Pluggy é obrigatória.", nameof(connectionId));
        if (recordType is not ("expense" or "revenue"))
            throw new ArgumentException("O tipo de lançamento é inválido.", nameof(recordType));
        if (recordId == Guid.Empty)
            throw new ArgumentException("O lançamento criado é obrigatório.", nameof(recordId));

        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        TransactionId = transactionId.Trim();
        ConnectionId = connectionId;
        RecordType = recordType;
        RecordId = recordId;
        RegisteredAt = registeredAt.ToUniversalTime();
    }
}
