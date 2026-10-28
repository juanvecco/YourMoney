namespace YourMoney.Application.DTOs;

public sealed class PluggyOverviewFilterDTO
{
    public Guid? ConnectionId { get; set; }
    public string? AccountId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

public sealed class PluggyOverviewDTO
{
    public DateTimeOffset GeneratedAt { get; set; }
    public DateTimeOffset? SourceUpdatedAt { get; set; }
    public IReadOnlyList<PluggyAccountDTO> Accounts { get; set; } = Array.Empty<PluggyAccountDTO>();
    public IReadOnlyList<PluggyBalanceGroupDTO> BalancesByCurrencyAndType { get; set; } = Array.Empty<PluggyBalanceGroupDTO>();
    public IReadOnlyList<PluggyTransactionPreviewDTO> Transactions { get; set; } = Array.Empty<PluggyTransactionPreviewDTO>();
}

public sealed class PluggyBalanceGroupDTO
{
    public string CurrencyCode { get; set; } = "BRL";
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int AccountCount { get; set; }
}

public sealed class PluggyDuplicateCandidateDTO
{
    public string RecordType { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
