namespace YourMoney.Application.DTOs;

public sealed class PluggyInvestmentAssetReconciliationRequestDTO
{
    public Guid ConnectionId { get; set; }
    public string ExternalInvestmentId { get; set; } = string.Empty;
    public string SnapshotToken { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? TargetInvestmentId { get; set; }
    public string? ExpectedLocalName { get; set; }
    public decimal? ExpectedLocalValue { get; set; }
    public CriarInvestimentoRequest? Investment { get; set; }
}

public sealed class PluggyInvestmentAssetReconciliationResultDTO
{
    public string ExternalInvestmentId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid InvestmentId { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
}
