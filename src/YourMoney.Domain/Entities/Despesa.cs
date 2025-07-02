using System;
using System.Collections.Generic;
using YourMoney.Domain.Enums;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Domain.Entities
{
    public class Despesa : BaseEntity
    {
        public string Descricao { get; private set; }
        public Money Valor { get; private set; }
        public DateTime Data { get; private set; }
        public Guid CategoriaId { get; private set; }
        public virtual Categoria Categoria { get; private set; }
        public bool Pago { get; private set; }
        public DateTime? DataPagamento { get; private set; }
        public TipoRecorrencia TipoRecorrencia { get; private set; }
        public DateTime DataCriacao { get; private set; }

        private Despesa() { } // Para ORM

        public Despesa(string descricao, Money valor, DateTime data, Guid categoriaId, TipoRecorrencia tipoRecorrencia = TipoRecorrencia.Unica)
        {
            Id = Guid.NewGuid();
            AtualizarDescricao(descricao);
            AtualizarValor(valor);
            AtualizarData(data);
            CategoriaId = categoriaId;
            TipoRecorrencia = tipoRecorrencia;
            Pago = false;
            DataCriacao = DateTime.Now;
        }

        public void AtualizarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória");
            Descricao = descricao.Trim();
        }

        public void AtualizarValor(Money valor)
        {
            Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        }

        public void AtualizarData(DateTime data)
        {
            Data = data;
        }
        public void AtualizarCategoriaId(Guid categoriaId)
        {
            if (categoriaId == Guid.Empty)
                throw new ArgumentException("CategoriaId é obrigatório.");
            CategoriaId = categoriaId;
        }

        public void MarcarComoPaga()
        {
            Pago = true;
            DataPagamento = DateTime.Now;
        }

        public void DesmarcarPagamento()
        {
            Pago = false;
            DataPagamento = null;
        }
    }
}