using YourMoney.Application.Commands.Requests;
using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;
using MediatR;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Application.Commands.Handlers
{
    public class CriarDespesaCommandHandler : IRequestHandler<CriarDespesaCommand, Guid>
    {
        private readonly IDespesaRepository _despesaRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public CriarDespesaCommandHandler(IDespesaRepository despesaRepository, ICategoriaRepository categoriaRepository)
        {
            _despesaRepository = despesaRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<Guid> Handle(CriarDespesaCommand request, CancellationToken cancellationToken)
        {
            //if (!await _categoriaRepository.ExisteAsync(request.CategoriaId))
            //    throw new InvalidOperationException("Categoria não encontrada.");

            // Fixed the issue by using the correct type for 'Valor'  
            var despesa = new Despesa(
                request.Descricao,
                request.Valor, // Assuming 'Money' has an 'Amount' property for decimal value  
                request.Data,
                request.IdContaFinanceira
            //request.CategoriaId,
            //request.TipoRecorrencia
            );

            await _despesaRepository.AdicionarAsync(despesa);
            return despesa.Id;
        }
    }

}
