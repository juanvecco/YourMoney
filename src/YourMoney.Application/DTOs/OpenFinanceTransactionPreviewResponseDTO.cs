namespace YourMoney.Application.DTOs
{
    public class OpenFinanceTransactionPreviewResponseDTO
    {
        public string? SourceId { get; set; }
        public string Mode { get; set; } = "preview-only";
        public List<OpenFinanceTransactionPreviewDTO> Items { get; set; } = new();
    }
}
