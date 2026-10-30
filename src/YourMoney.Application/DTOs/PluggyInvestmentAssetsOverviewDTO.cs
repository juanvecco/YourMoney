namespace YourMoney.Application.DTOs;

public sealed class PluggyInvestmentAssetsOverviewDTO
{
    public DateTimeOffset GeneratedAt { get; set; }
    public List<PluggyInvestmentAssetConnectionDTO> Connections { get; set; } = [];
    public List<PluggyInvestmentAssetPreviewDTO> Assets { get; set; } = [];
}

public sealed class PluggyInvestmentAssetConnectionDTO
{
    public Guid ConnectionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public bool HasInvestmentProduct { get; set; }
    public string Status { get; set; } = "never";
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? LastSuccessAt { get; set; }
    public DateTimeOffset? NextRefreshAt { get; set; }
    public DateTimeOffset? RetryAfterAt { get; set; }
    public string? Message { get; set; }
    public int AssetCount { get; set; }
}

// The application response uses its own contract. Provider JSON aliases such as
// "id", "date" and "value" belong only to PluggyInvestmentAssetDTO.
public sealed class PluggyInvestmentAssetPreviewDTO
{
    public string InvestmentId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Subtype { get; set; }
    public string? Code { get; set; }
    public string? Isin { get; set; }
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public decimal? CurrentUnitQuote { get; set; }
    public decimal? OriginalAmount { get; set; }
    public DateTimeOffset PositionDate { get; set; }
    public DateTimeOffset? PurchaseDate { get; set; }
    public string? Issuer { get; set; }
    public string? IssuerCnpj { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? SourceUpdatedAt { get; set; }
    public string SnapshotToken { get; set; } = string.Empty;
    public bool IsStale { get; set; }
    public bool CanReconcile { get; set; }
    public string? ReconcileUnavailableReason { get; set; }
    public Guid? LinkedInvestmentId { get; set; }
    public string? LinkedInvestmentName { get; set; }
    public List<PluggyInvestmentAssetCandidateDTO> Candidates { get; set; } = [];
}

public sealed class PluggyInvestmentAssetCandidateDTO
{
    public Guid InvestmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Isin { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal ValorAtual { get; set; }
    public decimal? PrecoUnitarioAtual { get; set; }
    public DateTime DataInvestimento { get; set; }
    public DateTime? MesReferencia { get; set; }
    public Guid? ReceitaRecorrenteId { get; set; }
    public Guid? GrupoObjetivoId { get; set; }
    public string? TipoAtivo { get; set; }
    public bool CanReplace { get; set; }
    public string? CannotReplaceReason { get; set; }
}
