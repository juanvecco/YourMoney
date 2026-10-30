using System.ComponentModel.DataAnnotations;

namespace YourMoney.Application.DTOs;

public sealed class PluggyTransactionImportRequestDTO
{
    [Required]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    public string RecordType { get; set; } = string.Empty;

    [Required, StringLength(255, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public Guid? IdContaFinanceira { get; set; }
    public Guid? IdCategoria { get; set; }
    public string? Natureza { get; set; }
    public Guid? DespesaVinculadaId { get; set; }
}

public sealed class PluggyTransactionImportResultDTO
{
    public string TransactionId { get; set; } = string.Empty;
    public string RecordType { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
}
