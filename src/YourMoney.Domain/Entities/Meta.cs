using System;
using YourMoney.Domain.ValueObjects;
using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class Meta : BaseEntity
    {
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public decimal ValorObjetivo { get; private set; }
        public decimal ValorAtual { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataObjetivo { get; private set; }
        public StatusMeta Status { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public virtual Categoria Categoria { get; private set; }

        private Meta() { }

        public Meta(string nome, string descricao, decimal valorObjetivo, DateTime dataObjetivo, Guid? categoriaId = null)
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarDescricao(descricao);
            ValorObjetivo = valorObjetivo;
            ValorAtual = 0;
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

        public void AdicionarValor(decimal valor)
        {
            ValorAtual += valor;
            VerificarConclusao();
        }

        public void SubtrairValor(decimal valor)
        {
            ValorAtual -= valor;
        }

        public decimal PercentualConcluido()
        {
            if (ValorObjetivo == 0) return 0;
            return Math.Min((ValorAtual / ValorObjetivo) * 100, 100);
        }

        public void PausarMeta() => Status = StatusMeta.Pausada;
        public void ReativarMeta() => Status = StatusMeta.Ativa;
        public void CancelarMeta() => Status = StatusMeta.Cancelada;

        private void VerificarConclusao()
        {
            if (ValorAtual >= ValorObjetivo && Status == StatusMeta.Ativa)
            {
                Status = StatusMeta.Concluida;
            }
        }
    }
}