namespace YourMoney.Domain.Entities
{
    public class MetaMensal : BaseEntity
    {
        public string Nome { get; private set; } = string.Empty;
        public decimal PercentualReceita { get; private set; }
        public DateTime MesReferencia { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime? AtualizadoEm { get; private set; }

        private MetaMensal() { }

        public MetaMensal(string nome, decimal percentualReceita, DateTime mesReferencia)
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarPercentualReceita(percentualReceita);
            AtualizarMesReferencia(mesReferencia);
            CriadoEm = DateTime.UtcNow;
        }

        public MetaMensal(string nome, decimal percentualReceita, DateTime mesReferencia, string usuarioId)
            : this(nome, percentualReceita, mesReferencia)
        {
            DefinirUsuario(usuarioId);
        }

        public void Atualizar(string nome, decimal percentualReceita)
        {
            AtualizarNome(nome);
            AtualizarPercentualReceita(percentualReceita);
            AtualizadoEm = DateTime.UtcNow;
        }

        public void AtualizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome da meta é obrigatório.", nameof(nome));

            var nomeNormalizado = nome.Trim();
            if (nomeNormalizado.Length > 100)
                throw new ArgumentException("Nome da meta deve ter no máximo 100 caracteres.", nameof(nome));

            Nome = nomeNormalizado;
        }

        public void AtualizarPercentualReceita(decimal percentualReceita)
        {
            if (percentualReceita <= 0)
                throw new ArgumentException("Percentual da receita deve ser maior que zero.", nameof(percentualReceita));

            PercentualReceita = percentualReceita;
        }

        public void AtualizarMesReferencia(DateTime mesReferencia)
        {
            if (mesReferencia == default)
                throw new ArgumentException("Mês de referência é obrigatório.", nameof(mesReferencia));

            MesReferencia = new DateTime(mesReferencia.Year, mesReferencia.Month, 1);
        }
    }
}
