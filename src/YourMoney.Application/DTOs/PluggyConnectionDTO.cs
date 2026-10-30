namespace YourMoney.Application.DTOs;

public sealed class PluggyConnectionDTO
{
    public Guid Id { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsSandbox { get; set; }
    public DateTimeOffset? LastUpdatedAt { get; set; }
    public DateTimeOffset? ConsentExpiresAt { get; set; }
    public DateTimeOffset ConnectedAt { get; set; }
}

public sealed class PluggyConnectionRequestDTO
{
    public string ItemId { get; set; } = string.Empty;
    public Guid? ConnectionId { get; set; }
}

public sealed class PluggyAccountDTO
{
    public Guid ConnectionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Number { get; set; }
    public decimal? Balance { get; set; }
    public string CurrencyCode { get; set; } = "BRL";
    public DateTimeOffset? LastUpdatedAt { get; set; }
}

public sealed class PluggyTransactionPreviewDTO
{
    public string TransactionId { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "BRL";
    public decimal? AmountInAccountCurrency { get; set; }
    public string AccountCurrencyCode { get; set; } = "BRL";
    public string Direction { get; set; } = "UNKNOWN";
    public string? OperationType { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool PreviewOnly { get; set; } = true;
    public string RegistrationStatus { get; set; } = "PENDING";
    public string? RegisteredRecordType { get; set; }
    public Guid? RegisteredRecordId { get; set; }
    public bool PossibleDuplicate { get; set; }
    public PluggyDuplicateCandidateDTO? DuplicateCandidate { get; set; }
    public IReadOnlyList<PluggyDuplicateCandidateDTO> PossibleDuplicateCandidates { get; set; } =
        Array.Empty<PluggyDuplicateCandidateDTO>();
    public bool Importable { get; set; }
    public decimal? SuggestedAmountBrl { get; set; }
    public string? ImportUnavailableReason { get; set; }
}
