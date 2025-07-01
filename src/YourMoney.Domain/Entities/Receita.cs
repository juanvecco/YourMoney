using System;
using YourMoney.Domain.ValueObjects;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class Receita : BaseEntity
    {
        public Guid Id { get; private set; }
        public string Descricao { get; private set; }
        public Money Valor { get; private set; }
        public DateTime Data { get; private set; }
        public Guid CategoriaId { get; private set; }
        public virtual Categoria Categoria { get; private set; }
        public bool Recebida { get; private set; }
        public DateTime? DataRecebimento { get; private set; }
        public TipoRecorrencia TipoRecorrencia { get; private set; }
        public DateTime DataCriacao { get; private set; }

        private Receita() { }

        public Receita(string descricao, Money valor, DateTime data, Guid categoriaId, TipoRecorrencia tipoRecorrencia = TipoRecorrencia.Unica)
        {
            Id = Guid.NewGuid();
            AtualizarDescricao(descricao);
            AtualizarValor(valor);
            AtualizarData(data);
            CategoriaId = categoriaId;
            TipoRecorrencia = tipoRecorrencia;
            Recebida = false;
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

        public void MarcarComoRecebida()
        {
            Recebida = true;
            DataRecebimento = DateTime.Now;
        }

        public void DesmarcarRecebimento()
        {
            Recebida = false;
            DataRecebimento = null;
        }
    }
}