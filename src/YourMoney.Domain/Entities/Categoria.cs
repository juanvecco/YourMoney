using System;
using System.Collections.Generic;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class Categoria : BaseEntity
    {
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public string Cor { get; private set; }
        public string Icone { get; private set; }
        public TipoTransacao TipoTransacao { get; private set; }
        public bool Ativa { get; private set; }
        public DateTime DataCriacao { get; private set; }

        private Categoria() { }

        public Categoria(string nome, string descricao, TipoTransacao tipoTransacao, string cor = "#007bff", string icone = "fa-circle")
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarDescricao(descricao);
            TipoTransacao = tipoTransacao;
            Cor = cor;
            Icone = icone;
            Ativa = true;
            DataCriacao = DateTime.Now;
        }

        public void AtualizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome da categoria é obrigatório");

            Nome = nome.Trim();
        }

        public void AtualizarDescricao(string descricao)
        {
            Descricao = descricao?.Trim() ?? string.Empty;
        }

        public void Ativar() => Ativa = true;
        public void Desativar() => Ativa = false;
    }
}