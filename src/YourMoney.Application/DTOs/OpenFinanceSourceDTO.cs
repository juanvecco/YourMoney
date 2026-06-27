namespace YourMoney.Application.DTOs
{
    public class OpenFinanceSourceDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? LastCheckedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool SupportsTransactionPreview { get; set; }
    }
}
