using System.Text.Json.Serialization;

namespace YourMoney.Application.DTOs;

/// <summary>
/// Normalized Pluggy investment position enriched with its authenticated connection.
/// The property names for provider values preserve Pluggy's original semantics.
/// </summary>
public class PluggyInvestmentAssetDTO
{
    [JsonPropertyName("id")]
    public string InvestmentId { get; set; } = string.Empty;

    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;

    public Guid ConnectionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("isin")]
    public string? Isin { get; set; }

    /// <summary>Net current position value, in CurrencyCode.</summary>
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    /// <summary>Current unit/quota quote; this is not the acquisition average price.</summary>
    [JsonPropertyName("value")]
    public decimal? CurrentUnitQuote { get; set; }

    [JsonPropertyName("amount")]
    public decimal? GrossAmount { get; set; }

    [JsonPropertyName("amountOriginal")]
    public decimal? OriginalAmount { get; set; }

    [JsonPropertyName("taxes")]
    public decimal? Taxes { get; set; }

    [JsonPropertyName("taxes2")]
    public decimal? Taxes2 { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset PositionDate { get; set; }

    [JsonPropertyName("purchaseDate")]
    public DateTimeOffset? PurchaseDate { get; set; }

    [JsonPropertyName("issueDate")]
    public DateTimeOffset? IssueDate { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTimeOffset? DueDate { get; set; }

    [JsonPropertyName("gracePeriodDate")]
    public DateTimeOffset? GracePeriodDate { get; set; }

    [JsonPropertyName("issuer")]
    public string? Issuer { get; set; }

    [JsonPropertyName("issuerCNPJ")]
    public string? IssuerCnpj { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("rate")]
    public decimal? Rate { get; set; }

    [JsonPropertyName("rateType")]
    public string? RateType { get; set; }

    [JsonPropertyName("fixedAnnualRate")]
    public decimal? FixedAnnualRate { get; set; }

    [JsonPropertyName("taxExempt")]
    public bool? TaxExempt { get; set; }

    [JsonPropertyName("indexerAdditionalInfo")]
    public string? IndexerAdditionalInfo { get; set; }

    public DateTimeOffset? SourceUpdatedAt { get; set; }
}
