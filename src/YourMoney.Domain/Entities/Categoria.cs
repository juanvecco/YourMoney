using System;
using System.Collections.Generic;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class Categoria : BaseEntity
    {
        public string Descricao { get; private set; }
        public TipoTransacao TipoTransacao { get; private set; }
        public Guid? CategoriaPaiId { get; private set; }
        public Categoria CategoriaPai { get; private set; }
        //public ICollection<Categoria> Subcategorias { get; private set; }

        private Categoria() { }

        public Categoria(string descricao, TipoTransacao tipoTransacao, Guid? categoriaPaiId = null)
        {
            Id = Guid.NewGuid();
            AtualizarDescricao(descricao);
            TipoTransacao = tipoTransacao;
            CategoriaPaiId = categoriaPaiId;
        }

        public void AtualizarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória", nameof(descricao));

            Descricao = descricao.Trim();
        }

        //public void AtualizarDescricao(string descricao)
        //{
        //    Descricao = descricao?.Trim() ?? string.Empty;
        //}

        //public void Ativar() => Ativa = true;
        //public void Desativar() => Ativa = false;
    }
}