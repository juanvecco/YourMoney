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
        public Guid? GrupoObjetivoInvestimentoId { get; set; }
        public string? ObjetivoNome { get; set; }
        public decimal ValorConfirmado { get; set; }
        public decimal ValorRestante { get; set; }
        public decimal Diferenca { get; set; }
        public string StatusPlanejamento { get; set; } = "AbaixoPlanejado";
        public decimal ValorAguardandoAlocacao { get; set; }
        public List<AporteObjetivoInvestimentoDTO> Aportes { get; set; } = new();
    }
}
