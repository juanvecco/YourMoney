namespace YourMoney.Application.DTOs
{
    public class CriarMetaMensalDTO
    {
        public string? Nome { get; set; }
        public string? TipoDefinicao { get; set; }
        public decimal? PercentualReceita { get; set; }
        public decimal? ValorMeta { get; set; }
        public DateTime? MesReferencia { get; set; }
    }
}
