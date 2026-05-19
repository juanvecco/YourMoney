using YourMoney.Application.Commands.Requests;
using YourMoney.Application.Queries.Responses;
using YourMoney.Domain.Repositories;

namespace YourMoney.Application.Commands.Handlers
{
    public class DespesasPorTipoMesAnoQueryHandler
    {
        private readonly IDespesaRepository _repository;

        public DespesasPorTipoMesAnoQueryHandler(IDespesaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DespesaResponse>> Handle(DespesasPorTipoMesAnoQuery query)
        {
            var despesas = await _repository.GetByTipoMesAnoAsync(
                query.IdTipoDespesa,
                query.Mes,
                query.Ano
            );

            return despesas.Select(d => new DespesaResponse
            {
                Id = d.Id,
                Descricao = d.Descricao,
                Valor = d.Valor,
                Data = d.Data,
                TipoDespesa = d.TipoDespesa.txtDescricao
            }).ToList();
        }
    }


}
