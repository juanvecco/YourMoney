using System;
using YourMoney.Domain.ValueObjects;

namespace YourMoney.Domain.Entities
{
    public class Investimento : BaseEntity
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public Money ValorInvestido { get; private set; }
        public Money ValorAtual { get; private set; }
        public DateTime DataInvestimento { get; private set; }
        public DateTime? DataResgate { get; private set; }
        public string TipoInvestimento { get; private set; }
        public decimal TaxaRetorno { get; private set; }
        public bool Ativo { get; private set; }

        private Investimento() { }

        public Investimento(string nome, string descricao, Money valorInvestido, DateTime dataInvestimento, string tipoInvestimento)
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarDescricao(descricao);
            ValorInvestido = valorInvestido ?? throw new ArgumentNullException(nameof(valorInvestido));
            ValorAtual = valorInvestido;
            DataInvestimento = dataInvestimento;
            TipoInvestimento = tipoInvestimento ?? throw new ArgumentNullException(nameof(tipoInvestimento));
            TaxaRetorno = 0;
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

        public void AtualizarValorAtual(Money novoValor)
        {
            ValorAtual = novoValor ?? throw new ArgumentNullException(nameof(novoValor));
            CalcularTaxaRetorno();
        }

        public void Resgatar()
        {
            Ativo = false;
            DataResgate = DateTime.Now;
        }

        private void CalcularTaxaRetorno()
        {
            if (ValorInvestido.Valor > 0)
            {
                TaxaRetorno = ((ValorAtual.Valor - ValorInvestido.Valor) / ValorInvestido.Valor) * 100;
            }
        }
    }
}