namespace YourMoney.Application.Queries.Responses
{
    public class DespesaResponse
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Categoria { get; set; }
    
    }
}
