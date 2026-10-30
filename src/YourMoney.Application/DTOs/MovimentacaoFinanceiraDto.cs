namespace YourMoney.Application.DTOs;

public sealed class ConsultaMovimentacoesRequest
{
    public string MesInicial { get; set; } = string.Empty;
    public string MesFinal { get; set; } = string.Empty;
    public string? Tipo { get; set; }
    public Guid? ContaFinanceiraId { get; set; }
    public string? Busca { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 25;
}

public sealed class MovimentacaoFinanceiraDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public DateTime MesReferencia { get; set; }
    public decimal Valor { get; set; }
    public Guid? ContaFinanceiraId { get; set; }
    public string? ContaFinanceiraNome { get; set; }
    public string? Categoria { get; set; }
    public string RotaOrigem { get; set; } = string.Empty;
}

public sealed class ResumoFinanceiroPeriodoDto
{
    public decimal Receitas { get; set; }
    public decimal Despesas { get; set; }
    public decimal Investimentos { get; set; }
    public decimal Saldo => Receitas - Despesas;
}

public sealed class ResumoFinanceiroMensalDto
{
    public string MesReferencia { get; set; } = string.Empty;
    public int TotalItens { get; set; }
    public decimal Receitas { get; set; }
    public decimal Despesas { get; set; }
    public decimal Investimentos { get; set; }
    public decimal Saldo => Receitas - Despesas;
}

public sealed class ConsultaMovimentacoesResponse
{
    public List<MovimentacaoFinanceiraDto> Itens { get; set; } = new();
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
    public ResumoFinanceiroPeriodoDto Totais { get; set; } = new();
    public List<ResumoFinanceiroMensalDto> TotaisPorMes { get; set; } = new();
}
