namespace YourMoney.Application.DTOs
{
    public class OpenFinanceReadinessDTO
    {
        public string Mode { get; set; } = "preview-only";
        public bool RealDataEnabled { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> NextSteps { get; set; } = new();
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}
