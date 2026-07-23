namespace YourMoney.Application.DTOs
{
    public class InvestimentoDto
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? Tipo { get; set; }
        public decimal Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime DataInvestimento { get; set; }
        public DateTime? MesReferencia { get; set; }
        public DateTime? DataResgate { get; set; }
        public bool Ativo { get; set; }
        public Guid? ReceitaRecorrenteId { get; set; }
    }

    public class InvestimentoWriteRequest
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? Tipo { get; set; }
        public decimal Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime DataInvestimento { get; set; }
        public DateTime MesReferencia { get; set; }
        public Guid? ReceitaRecorrenteId { get; set; }
    }

    public class CriarInvestimentoRequest : InvestimentoWriteRequest
    {
        public Guid OperacaoId { get; set; }
    }

    public class AtualizarInvestimentoRequest : InvestimentoWriteRequest
    {
    }

    public class InvestimentoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime DataInvestimento { get; set; }
        public DateTime? MesReferencia { get; set; }
        public DateTime? DataResgate { get; set; }
        public bool Ativo { get; set; }
        public Guid? ReceitaRecorrenteId { get; set; }
        public ReservaAssociadaInvestimentoResponse? ReservaAssociada { get; set; }
    }

    public class CriarInvestimentoResponse : InvestimentoResponse
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public bool CriadoAgora { get; set; } = true;
    }

    public class ReservaAssociadaInvestimentoResponse
    {
        public string Descricao { get; set; } = string.Empty;
        public string ContaDescricao { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public bool ElegivelParaNovaAssociacao { get; set; }
    }

    public class ProgressoMetaReservaResponse
    {
        public int Meses { get; set; }
        public decimal ValorMeta { get; set; }
        public decimal ValorRestante { get; set; }
        public decimal PercentualAlcancado { get; set; }
    }

    public class ReservaSalarialResponse
    {
        public Guid ReceitaRecorrenteId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string ContaDescricao { get; set; } = string.Empty;
        public decimal ValorMensal { get; set; }
        public decimal ValorAcumulado { get; set; }
        public bool Ativa { get; set; }
        public bool ElegivelParaNovaAssociacao { get; set; }
        public ProgressoMetaReservaResponse MetaSeisMeses { get; set; } = new();
        public ProgressoMetaReservaResponse MetaDozeMeses { get; set; } = new();
    }

    public class CarteiraInvestimentosConsolidadaResponse
    {
        public decimal TotalInvestido { get; set; }
        public List<InvestimentoResponse> Itens { get; set; } = new();
        public List<ReservaSalarialResponse> Reservas { get; set; } = new();
    }
}
