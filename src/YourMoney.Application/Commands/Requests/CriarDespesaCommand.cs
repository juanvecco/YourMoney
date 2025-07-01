using MediatR;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Application.Commands.Requests
{
    public class CriarDespesaCommand : IRequest<Guid>
    {
        public string? Descricao { get; set; }
        public Money Valor { get; set; }
        public DateTime Data { get; set; }
        public Guid CategoriaId { get; set; }
    }
}
