using System;
using YourMoney.Domain.Enums;

namespace YourMoney.Application.DTOs
{
    public class DespesaDTO
    {
        public Guid Id { get; set; }
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public Guid CategoriaId { get; set; }
        public bool Pago { get; set; }
        public DateTime? DataPagamento { get; set; }
        public TipoRecorrencia TipoRecorrencia { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}