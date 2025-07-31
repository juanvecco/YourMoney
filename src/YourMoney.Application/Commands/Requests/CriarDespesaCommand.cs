using MediatR;
using YourMoney.Domain.Enums;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Application.Commands.Requests
{
    public class CriarDespesaCommand : IRequest<Guid>
    {
        public string? Descricao { get; set; }
        public Decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public Guid IdContaFinanceira { get; internal set; }
        public Guid IdCategoria { get; internal set; }
        //public TipoRecorrencia TipoRecorrencia { get; set; }
    }
}
