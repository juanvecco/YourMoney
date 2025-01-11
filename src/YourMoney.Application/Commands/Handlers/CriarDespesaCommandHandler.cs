using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourMoney.Application.Commands.Requests;
using YourMoney.Domain.Repositories;
using YourMoney.Domain.Entities;
using MediatR;

namespace YourMoney.Application.Commands.Handlers
{
    public class CriarDespesaCommandHandler : IRequestHandler<CriarDespesaCommand, Guid>
    {
        private readonly IDespesaRepository _repository;

        public CriarDespesaCommandHandler(IDespesaRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CriarDespesaCommand request, CancellationToken cancellationToken)
        {
            var despesa = new Despesa(request.Descricao, request.Valor, request.Data, request.Categoria);
            await _repository.AdicionarAsync(despesa);

            return despesa.Id;
        }
    }

}
