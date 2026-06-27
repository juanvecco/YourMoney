namespace YourMoney.Application.DTOs
{
    public class OpenFinanceSourcesResponseDTO
    {
        public OpenFinanceReadinessDTO Readiness { get; set; } = new();
        public List<OpenFinanceSourceDTO> Sources { get; set; } = new();
    }
}
