using System;
using YourMoney.Domain.ValueObjects;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class Meta : BaseEntity
    {
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public Money ValorObjetivo { get; private set; }
        public Money ValorAtual { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataObjetivo { get; private set; }
        public StatusMeta Status { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public virtual Categoria Categoria { get; private set; }

        private Meta() { }

        public Meta(string nome, string descricao, Money valorObjetivo, DateTime dataObjetivo, Guid? categoriaId = null)
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarDescricao(descricao);
            ValorObjetivo = valorObjetivo ?? throw new ArgumentNullException(nameof(valorObjetivo));
            ValorAtual = new Money(0, valorObjetivo.Moeda);
            DataInicio = DateTime.Now;
            DataObjetivo = dataObjetivo;
            Status = StatusMeta.Ativa;
            CategoriaId = categoriaId;
        }

        public void AtualizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome da meta é obrigatório");
            Nome = nome.Trim();
        }

        public void AtualizarDescricao(string descricao)
        {
            Descricao = descricao?.Trim() ?? string.Empty;
        }

        public void AdicionarValor(Money valor)
        {
            ValorAtual = ValorAtual.Somar(valor);
            VerificarConclusao();
        }

        public void SubtrairValor(Money valor)
        {
            ValorAtual = ValorAtual.Subtrair(valor);
        }

        public decimal PercentualConcluido()
        {
            if (ValorObjetivo.Valor == 0) return 0;
            return Math.Min((ValorAtual.Valor / ValorObjetivo.Valor) * 100, 100);
        }

        public void PausarMeta() => Status = StatusMeta.Pausada;
        public void ReativarMeta() => Status = StatusMeta.Ativa;
        public void CancelarMeta() => Status = StatusMeta.Cancelada;

        private void VerificarConclusao()
        {
            if (ValorAtual.Valor >= ValorObjetivo.Valor && Status == StatusMeta.Ativa)
            {
                Status = StatusMeta.Concluida;
            }
        }
    }
}