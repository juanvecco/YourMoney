namespace YourMoney.Application.DTOs
{
    public class DashboardDTO
    {
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal TotalInvestimentos { get; set; }
        public decimal SaldoMensal { get; set; }
        public List<MetaResumoDTO> MetasAtivas { get; set; } = new();
        public List<ContaVencendoDTO> ContasVencendo { get; set; } = new();
        public List<GraficoDTO> GraficoDespesas { get; set; } = new();
        public List<GraficoDTO> GraficoReceitas { get; set; } = new();
    }

    public class MetaResumoDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public decimal ValorObjetivo { get; set; }
        public decimal ValorAtual { get; set; }
        public decimal PercentualConcluido { get; set; }
        public DateTime DataObjetivo { get; set; }
    }

    public class ContaVencendoDTO
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public int DiasVencimento { get; set; }
    }

    public class GraficoDTO
    {
        public string Label { get; set; }
        public decimal Valor { get; set; }
        public string Cor { get; set; }
    }

    public class BalancoMensalDTO
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal TotalInvestimentos { get; set; }
        public decimal SaldoFinal { get; set; }
        public List<CategoriaResumoDTO> DespesasPorCategoria { get; set; } = new();
        public List<CategoriaResumoDTO> ReceitasPorCategoria { get; set; } = new();
    }

    public class CategoriaResumoDTO
    {
        public Guid CategoriaId { get; set; }
        public string NomeCategoria { get; set; }
        public decimal Total { get; set; }
        public string Cor { get; set; }
    }
}