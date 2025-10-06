using System;
using YourMoney.Domain.ValueObjects;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YourMoney.Domain.Entities
{
    public class Investimento : BaseEntity
    {
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public string Tipo { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal PrecoMedio { get; private set; }
        //public Decimal ValorInvestido { get; private set; }
        public Decimal ValorAtual { get; private set; }
        public DateTime DataInvestimento { get; private set; }
        public DateTime? DataResgate { get; private set; }
        //public decimal TaxaRetorno { get; private set; }
        public bool Ativo { get; private set; }

        private Investimento() { }

        public Investimento(string nome, string descricao, string tipo, decimal quantidade, decimal precoMedio, decimal valorAtual, DateTime dataInvestimento)
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarDescricao(descricao);
            Tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            Quantidade = quantidade;
            PrecoMedio = precoMedio;
            ValorAtual = valorAtual;
            DataInvestimento = dataInvestimento;
            Ativo = true;
        }

        public void AtualizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do investimento é obrigatório");
            Nome = nome.Trim();
        }

        public void AtualizarDescricao(string descricao)
        {
            Descricao = descricao?.Trim() ?? string.Empty;
        }

        public void AtualizarValorAtual(Decimal novoValor)
        {
            if (novoValor <= 0) // Adjusted condition to check for invalid values instead of null  
                throw new ArgumentException("O novo valor deve ser maior que zero.", nameof(novoValor));
            ValorAtual = novoValor;
            //CalcularTaxaRetorno();
        }

        public void AtualizarData(DateTime data)
        {
            DataInvestimento = data;
        }

        public void Resgatar()
        {
            Ativo = false;
            DataResgate = DateTime.Now;
        }
        public void AtualizarTipo(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("Tipo do investimento é obrigatório");
            Tipo = tipo.Trim();
        }

        public void AtualizarQuantidade(decimal quantidade)
        {
            if (quantidade <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero");
            Quantidade = quantidade;
        }

        public void AtualizarPrecoMedio(decimal precoMedio)
        {
            if (precoMedio <= 0)
                throw new ArgumentException("Preço médio deve ser maior que zero");
            PrecoMedio = precoMedio;
        }

        //private void CalcularTaxaRetorno()
        //{
        //    if (ValorInvestido > 0)
        //    {
        //        TaxaRetorno = ((ValorAtual - ValorInvestido) / ValorInvestido) * 100;
        //    }
        //}
    }
}