namespace YourMoney.Application.DTOs
{
    public class ConsultaDespesasRequest
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public Guid? IdContaFinanceira { get; set; }
        public Guid? IdTipoDespesa { get; set; }
        public Guid? IdNaturezaDespesa { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 10;
    }

    public class ConsultaDespesasResponse
    {
        public List<DespesaDTO> Itens { get; set; } = new();
        public int PaginaAtual { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalResultados { get; set; }
        public int TotalPaginas { get; set; }
        public decimal ValorTotalFiltrado { get; set; }
        public List<ConsultaDespesasTotalPorContaDTO> TotaisPorConta { get; set; } = new();
    }

    public class ConsultaDespesasTotalPorContaDTO
    {
        public Guid IdContaFinanceira { get; set; }
        public decimal Valor { get; set; }
    }
}
