namespace YourMoney.Application.DTOs
{
    public class MetasMensaisResumoDTO
    {
        public DateTime MesReferencia { get; set; }
        public decimal ReceitaTotal { get; set; }
        public decimal ReceitaTotalBruta { get; set; }
        public decimal ReceitaElegivelMetas { get; set; }
        public decimal ReceitaExcluidaMetas { get; set; }
        public decimal DespesaTotal { get; set; }
        public decimal DespesaTotalBruta { get; set; }
        public decimal DespesaTotalReembolsada { get; set; }
        public decimal? PercentualTotalComprometido { get; set; }
        public decimal ValorTotalReservado { get; set; }
        public decimal? PercentualRestante { get; set; }
        public decimal ValorRestanteAntesDespesas { get; set; }
        public decimal SaldoFinal { get; set; }
        public decimal ValorFaltante { get; set; }
        public string Status { get; set; } = "zerado";
        public List<string> Alertas { get; set; } = new();
        public List<MetaMensalDTO> Metas { get; set; } = new();
    }
}
