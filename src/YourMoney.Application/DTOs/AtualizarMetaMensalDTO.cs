namespace YourMoney.Application.DTOs
{
    public class AtualizarMetaMensalDTO
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public decimal PercentualReceita { get; set; }
    }
}
