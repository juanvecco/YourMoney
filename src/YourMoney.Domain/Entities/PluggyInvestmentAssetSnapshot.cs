namespace YourMoney.Domain.Entities;

/// <summary>Normalized last successful Pluggy investment position; never stores a raw provider payload.</summary>
public sealed class PluggyInvestmentAssetSnapshot : BaseEntity
{
    public Guid ConnectionId { get; private set; }
    public string PluggyItemId { get; private set; } = string.Empty;
    public string ExternalInvestmentId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string? Subtype { get; private set; }
    public string? Code { get; private set; }
    public string? Isin { get; private set; }
    public decimal Balance { get; private set; }
    public string CurrencyCode { get; private set; } = "UNKNOWN";
    public decimal? Quantity { get; private set; }
    public decimal? CurrentUnitQuote { get; private set; }
    public decimal? OriginalAmount { get; private set; }
    public DateTimeOffset PositionDate { get; private set; }
    public DateTimeOffset? PurchaseDate { get; private set; }
    public string? Issuer { get; private set; }
    public string? IssuerCnpj { get; private set; }
    public string? Status { get; private set; }
    public DateTimeOffset FetchedAt { get; private set; }
    public bool IsCurrent { get; private set; } = true;

    private PluggyInvestmentAssetSnapshot() { }

    public PluggyInvestmentAssetSnapshot(string usuarioId, Guid connectionId, string itemId,
        string externalInvestmentId, string name, string type, string? subtype, string? code,
        string? isin, decimal balance, string currencyCode, decimal? quantity,
        decimal? currentUnitQuote, decimal? originalAmount, DateTimeOffset positionDate,
        DateTimeOffset? purchaseDate, string? issuer, string? issuerCnpj, string? status,
        DateTimeOffset fetchedAt)
    {
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        Atualizar(connectionId, itemId, externalInvestmentId, name, type, subtype, code, isin,
            balance, currencyCode, quantity, currentUnitQuote, originalAmount, positionDate,
            purchaseDate, issuer, issuerCnpj, status, fetchedAt);
    }

    public void Atualizar(Guid connectionId, string itemId, string externalInvestmentId,
        string name, string type, string? subtype, string? code, string? isin, decimal balance,
        string currencyCode, decimal? quantity, decimal? currentUnitQuote, decimal? originalAmount,
        DateTimeOffset positionDate, DateTimeOffset? purchaseDate, string? issuer,
        string? issuerCnpj, string? status, DateTimeOffset fetchedAt)
    {
        if (connectionId == Guid.Empty || string.IsNullOrWhiteSpace(itemId)
            || string.IsNullOrWhiteSpace(externalInvestmentId) || string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A identidade e o nome da posição são obrigatórios.");
        ConnectionId = connectionId;
        PluggyItemId = itemId.Trim();
        ExternalInvestmentId = externalInvestmentId.Trim();
        Name = name.Trim();
        Type = string.IsNullOrWhiteSpace(type) ? "Não informado" : type.Trim();
        Subtype = Limitar(subtype, 100);
        Code = Limitar(code, 100);
        Isin = Limitar(isin, 20);
        Balance = balance;
        CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "UNKNOWN" : currencyCode.Trim().ToUpperInvariant();
        Quantity = quantity;
        CurrentUnitQuote = currentUnitQuote;
        OriginalAmount = originalAmount;
        PositionDate = positionDate;
        PurchaseDate = purchaseDate;
        Issuer = Limitar(issuer, 200);
        IssuerCnpj = Limitar(issuerCnpj, 20);
        Status = Limitar(status, 80);
        FetchedAt = fetchedAt;
        IsCurrent = true;
    }

    public void MarcarAusente() => IsCurrent = false;

    private static string? Limitar(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized[..Math.Min(normalized.Length, maxLength)];
    }
}
