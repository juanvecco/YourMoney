namespace YourMoney.Application.DTOs
{
    public class CriarReceitaRequest
    {
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public DateTime MesReferencia { get; set; }
    }

    public class CriarReceitaResponse
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public DateTime MesReferencia { get; set; }
    }
}
