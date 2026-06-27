namespace YourMoney.Application.DTOs
{
    public class OpenFinanceTransactionPreviewDTO
    {
        public string Id { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string? ExternalReference { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string SuggestedFinancialType { get; set; } = string.Empty;
        public bool DuplicateRisk { get; set; }
        public string? DuplicateReason { get; set; }
        public string ImportStatus { get; set; } = "preview-only";
    }
}
