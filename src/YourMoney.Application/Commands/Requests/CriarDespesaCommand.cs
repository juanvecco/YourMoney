﻿using MediatR;


namespace YourMoney.Application.Commands.Requests
{
    public class CriarDespesaCommand : IRequest<Guid>
    {
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Categoria { get; set; }
    }
}
