using YourMoney.Domain.Enums;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Application.Queries.Responses
{
    public class DespesaResponse
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
        public Decimal Valor { get; set; }
        public DateTime Data { get; set; }
        //public Guid CategoriaId { get; set; }
        //public string? CategoriaNome { get; set; }
        //public bool Pago { get; set; }
        //public DateTime? DataPagamento { get; set; }
        //public TipoRecorrencia TipoRecorrencia { get; set; }
        //public DateTime DataCriacao { get; set; }

    }
}
