using MediatR;
using YourMoney.Application.Queries.Responses;
using YourMoney.Domain.Repositories;

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
            var despesas = await _repository.ListarAsync();
            return despesas.Select(e => new DespesaResponse
            {
                Id = e.Id,
                Descricao = e.Descricao,
                Valor = e.Valor,
                Data = e.Data//,
                //CategoriaId = e.CategoriaId,
                //CategoriaNome = e.Categoria?.Nome ?? "Sem Categoria",
                //Pago = e.Pago,
                //DataPagamento = e.DataPagamento,
                //TipoRecorrencia = e.TipoRecorrencia,
                //DataCriacao = e.DataCriacao
            }).ToList();
        }
    }
}