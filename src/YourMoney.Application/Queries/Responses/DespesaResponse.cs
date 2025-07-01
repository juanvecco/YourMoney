using YourMoney.Domain.ValueObjects;

namespace YourMoney.Application.Queries.Responses
{
    public class DespesaResponse
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
        public Money Valor { get; set; }
        public DateTime Data { get; set; }
        public Guid CategoriaId { get; set; }

    }
}
