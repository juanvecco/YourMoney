namespace YourMoney.Application.DTOs
{
    public class MetaMensalDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string TipoDefinicao { get; set; } = "Percentual";
        public decimal? PercentualReceita { get; set; }
        public decimal? ValorMeta { get; set; }
        public decimal ValorCalculado { get; set; }
        public DateTime MesReferencia { get; set; }
    }
}
