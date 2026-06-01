using System;
using System.Collections.Generic;
using YourMoney.Domain.Enums;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Domain.Entities
{
    public class Despesa : BaseEntity
    {
        public string Descricao { get; private set; }
        public Decimal Valor { get; private set; }
        public DateTime Data { get; private set; }
        public Guid IdContaFinanceira { get; private set; }
        public virtual ContaFinanceira ContaFinanceira { get; private set; }
        public Guid IdCategoria { get; private set; }
        public virtual Categoria Categoria { get; private set; }
        public Guid? ParcelamentoId { get; private set; }
        public int? NumeroParcela { get; private set; }
        public int? TotalParcelas { get; private set; }
        public decimal? ValorTotalParcelamento { get; private set; }
        //public bool Pago { get; private set; }
        //public DateTime? DataPagamento { get; private set; }
        //public TipoRecorrencia TipoRecorrencia { get; private set; }
        //public DateTime DataCriacao { get; private set; }

        private Despesa() { } // Para ORM

        public Despesa(string descricao, Decimal valor, DateTime data, Guid idContaFinanceira, Guid idCategoria)
        {
            Id = Guid.NewGuid();
            AtualizarDescricao(descricao);
            AtualizarValor(valor);
            AtualizarData(data);
            AtualizarContaFinanceira(idContaFinanceira);
            AtualizarCategoria(idCategoria);
            //TipoRecorrencia = tipoRecorrencia;
            //Pago = false;
            //DataCriacao = DateTime.Now;
        }

        public Despesa(string descricao, Decimal valor, DateTime data, Guid idContaFinanceira, Guid idCategoria, string usuarioId)
            : this(descricao, valor, data, idContaFinanceira, idCategoria)
        {
            DefinirUsuario(usuarioId);
        }

        public void AtualizarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória");
            Descricao = descricao.Trim();
        }

        public void AtualizarValor(Decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));
            Valor = valor;
        }

        public void AtualizarData(DateTime data)
        {
            Data = data;
        }
        public void AtualizarContaFinanceira(Guid idContaFinanceira)
        {
            if (idContaFinanceira == Guid.Empty)
                throw new ArgumentException("Conta Financeira é obrigatória.");
            IdContaFinanceira = idContaFinanceira;
        }
        public void AtualizarCategoria(Guid idCategoria)
        {
            if (idCategoria == Guid.Empty)
                throw new ArgumentException("Categoria é obrigatório.");
            IdCategoria = idCategoria;
        }

        public void AplicarParcelamento(Guid parcelamentoId, int numeroParcela, int totalParcelas, decimal valorTotalParcelamento)
        {
            if (parcelamentoId == Guid.Empty)
                throw new ArgumentException("Parcelamento é obrigatório.", nameof(parcelamentoId));
            if (totalParcelas < 1 || totalParcelas > 120)
                throw new ArgumentException("Total de parcelas deve estar entre 1 e 120.", nameof(totalParcelas));
            if (numeroParcela < 1 || numeroParcela > totalParcelas)
                throw new ArgumentException("Número da parcela deve estar entre 1 e o total de parcelas.", nameof(numeroParcela));
            if (valorTotalParcelamento <= 0)
                throw new ArgumentException("Valor total do parcelamento deve ser maior que zero.", nameof(valorTotalParcelamento));

            ParcelamentoId = parcelamentoId;
            NumeroParcela = numeroParcela;
            TotalParcelas = totalParcelas;
            ValorTotalParcelamento = valorTotalParcelamento;
        }

        public void LimparParcelamento()
        {
            ParcelamentoId = null;
            NumeroParcela = null;
            TotalParcelas = null;
            ValorTotalParcelamento = null;
        }

        //public void MarcarComoPaga()
        //{
        //    Pago = true;
        //    DataPagamento = DateTime.Now;
        //}

        //public void DesmarcarPagamento()
        //{
        //    Pago = false;
        //    DataPagamento = null;
        //}
    }
}
