namespace YourMoney.Application.DTOs
{
    public class ParcelamentoDespesaRequest
    {
        public string? Descricao { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataInicial { get; set; }
        public int QuantidadeParcelas { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public Guid IdCategoria { get; set; }
    }

    public class ParcelaDespesaResponse : DespesaDTO
    {
    }

    public class ParcelamentoDespesaResponse
    {
        public Guid? ParcelamentoId { get; set; }
        public decimal ValorTotal { get; set; }
        public int QuantidadeParcelas { get; set; }
        public List<ParcelaDespesaResponse> Parcelas { get; set; } = new();
    }
}
