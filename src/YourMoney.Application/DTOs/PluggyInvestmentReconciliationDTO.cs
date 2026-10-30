using System.ComponentModel.DataAnnotations;

namespace YourMoney.Application.DTOs;

public sealed class PluggyInvestmentReconciliationRequestDTO
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string TransactionId { get; set; } = string.Empty;

    [Required, RegularExpression("^(create|link|replace)$")]
    public string Action { get; set; } = string.Empty;

    public Guid? TargetInvestmentId { get; set; }
    public string? ExpectedDescription { get; set; }
    public decimal? ExpectedAmount { get; set; }
    public DateOnly? ExpectedDate { get; set; }

    public CriarInvestimentoRequest? Investment { get; set; }

    [StringLength(500, MinimumLength = 1)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true)]
    public decimal? Amount { get; set; }

    public DateOnly? Date { get; set; }
    public bool ConfirmPossibleDuplicate { get; set; }
}

public sealed class PluggyInvestmentReconciliationResultDTO
{
    public string TransactionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string RecordType { get; set; } = "investment";
    public Guid RecordId { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
}
