using YourMoney.Application.Queries.Responses;
using YourMoney.Domain.Repositories;
using MediatR;


namespace YourMoney.Application.Queries.Handlers
{
    public class GetExpensesQuery : IRequest<List<DespesaResponse>> { }

    //public class DespesasQueryHandler : IRequestHandler<GetExpensesQuery, List<DespesaResponse>>
    //{
    //    private readonly IDespesaRepository _repository;

    //    public DespesasQueryHandler(IDespesaRepository repository)
    //    {
    //        _repository = repository;
    //    }
    //}
}
