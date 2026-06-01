namespace YourMoney.Application.DTOs
{
    public class CriarDespesaRequest
    {
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public DateTime MesReferencia { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public Guid IdCategoria { get; set; }
    }

    public class CriarDespesaResponse
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public DateTime MesReferencia { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public Guid IdCategoria { get; set; }
    }
}
