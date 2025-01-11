using YourMoney.Application.Queries.Responses;
using YourMoney.Domain.Repositories;
using MediatR;


namespace YourMoney.Application.Queries.Handlers
{
    public class GetExpensesQuery : IRequest<List<DespesaResponse>> { }

    public class DespesasQueryHandler : IRequestHandler<GetExpensesQuery, List<DespesaResponse>>
    {
        private readonly IDespesaRepository _repository;

        public DespesasQueryHandler(IDespesaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DespesaResponse>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
        {
            var despesas = await _repository.GetAllAsync();
            return despesas.Select(e => new DespesaResponse
            {
                Id = e.Id,
                Descricao = e.Descricao,
                Valor = e.Valor,
                Data = e.Data,
                Categoria = e.Categoria
            }).ToList();
        }
    }
}
