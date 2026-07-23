namespace YourMoney.Application.DTOs
{
    public class AtualizarMetaMensalDTO
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? TipoDefinicao { get; set; }
        public decimal? PercentualReceita { get; set; }
        public decimal? ValorMeta { get; set; }
    }
}
