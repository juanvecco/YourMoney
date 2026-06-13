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
    }

    public class CriarInvestimentoRequest
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? Tipo { get; set; }
        public decimal Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime DataInvestimento { get; set; }
        public DateTime MesReferencia { get; set; }
    }

    public class AtualizarInvestimentoRequest : CriarInvestimentoRequest
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
    }

    public class CriarInvestimentoResponse : InvestimentoResponse
    {
    }
}
