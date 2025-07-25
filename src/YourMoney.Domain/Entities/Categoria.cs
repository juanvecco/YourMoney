using System;
using System.Collections.Generic;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class Categoria : BaseEntity
    {
        public string Descricao { get; private set; }

        private Categoria() { }

        public Categoria(string descricao)
        {
            Id = Guid.NewGuid();
            //AtualizarNome(nome);
            AtualizarDescricao(descricao);
            //TipoTransacao = tipoTransacao;
            //Cor = cor;
            //Icone = icone;
            //Ativa = true;
            //DataCriacao = DateTime.Now;
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