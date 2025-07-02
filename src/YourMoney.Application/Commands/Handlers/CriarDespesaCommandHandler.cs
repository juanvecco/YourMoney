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
            if (!await _categoriaRepository.ExisteAsync(request.CategoriaId))
                throw new InvalidOperationException("Categoria não encontrada.");

            var despesa = new Despesa(
                request.Descricao,
                new Money(request.Valor.Valor, "BRL"),
                request.Data,
                request.CategoriaId,
                request.TipoRecorrencia
            );

            await _despesaRepository.AdicionarAsync(despesa);
            return despesa.Id;
        }
    }

}
