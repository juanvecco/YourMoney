using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class MetaMensal : BaseEntity
    {
        public string Nome { get; private set; } = string.Empty;
        public TipoDefinicaoMeta TipoDefinicao { get; private set; } = TipoDefinicaoMeta.Percentual;
        public decimal? PercentualReceita { get; private set; }
        public decimal? ValorMeta { get; private set; }
        public DateTime MesReferencia { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime? AtualizadoEm { get; private set; }

        private MetaMensal() { }

        public MetaMensal(string nome, decimal percentualReceita, DateTime mesReferencia)
            : this(nome, TipoDefinicaoMeta.Percentual, percentualReceita, null, mesReferencia)
        {
        }

        public MetaMensal(string nome, decimal percentualReceita, DateTime mesReferencia, string usuarioId)
            : this(nome, TipoDefinicaoMeta.Percentual, percentualReceita, null, mesReferencia, usuarioId)
        {
        }

        public MetaMensal(
            string nome,
            TipoDefinicaoMeta tipoDefinicao,
            decimal? percentualReceita,
            decimal? valorMeta,
            DateTime mesReferencia)
        {
            Id = Guid.NewGuid();
            AtualizarNome(nome);
            AtualizarDefinicao(tipoDefinicao, percentualReceita, valorMeta);
            AtualizarMesReferencia(mesReferencia);
            CriadoEm = DateTime.UtcNow;
        }

        public MetaMensal(
            string nome,
            TipoDefinicaoMeta tipoDefinicao,
            decimal? percentualReceita,
            decimal? valorMeta,
            DateTime mesReferencia,
            string usuarioId)
            : this(nome, tipoDefinicao, percentualReceita, valorMeta, mesReferencia)
        {
            DefinirUsuario(usuarioId);
        }

        public void Atualizar(string nome, decimal percentualReceita)
        {
            Atualizar(nome, TipoDefinicaoMeta.Percentual, percentualReceita, null);
        }

        public void Atualizar(
            string nome,
            TipoDefinicaoMeta tipoDefinicao,
            decimal? percentualReceita,
            decimal? valorMeta)
        {
            AtualizarNome(nome);
            AtualizarDefinicao(tipoDefinicao, percentualReceita, valorMeta);
            AtualizadoEm = DateTime.UtcNow;
        }

        public void Renomear(string nome)
        {
            AtualizarNome(nome);
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
            DefinirPorPercentual(percentualReceita);
        }

        public void DefinirPorPercentual(decimal percentualReceita)
        {
            if (percentualReceita <= 0)
                throw new ArgumentException("Percentual da receita deve ser maior que zero.", nameof(percentualReceita));
            if (percentualReceita != decimal.Round(percentualReceita, 4, MidpointRounding.AwayFromZero))
                throw new ArgumentException("Percentual da receita deve ter no máximo quatro casas decimais.", nameof(percentualReceita));

            TipoDefinicao = TipoDefinicaoMeta.Percentual;
            PercentualReceita = percentualReceita;
            ValorMeta = null;
        }

        public void DefinirPorValor(decimal valorMeta)
        {
            if (valorMeta <= 0)
                throw new ArgumentException("Valor da meta deve ser maior que zero.", nameof(valorMeta));
            if (valorMeta != decimal.Round(valorMeta, 2, MidpointRounding.AwayFromZero))
                throw new ArgumentException("Valor da meta deve ter no máximo duas casas decimais.", nameof(valorMeta));

            TipoDefinicao = TipoDefinicaoMeta.Valor;
            ValorMeta = valorMeta;
            PercentualReceita = null;
        }

        public bool PossuiMesmaDefinicao(
            TipoDefinicaoMeta tipoDefinicao,
            decimal? percentualReceita,
            decimal? valorMeta)
        {
            return tipoDefinicao == TipoDefinicao
                && (tipoDefinicao == TipoDefinicaoMeta.Percentual
                    ? PercentualReceita == percentualReceita
                    : ValorMeta == valorMeta);
        }

        public void AtualizarMesReferencia(DateTime mesReferencia)
        {
            if (mesReferencia == default)
                throw new ArgumentException("Mês de referência é obrigatório.", nameof(mesReferencia));

            MesReferencia = new DateTime(mesReferencia.Year, mesReferencia.Month, 1);
        }

        private void AtualizarDefinicao(
            TipoDefinicaoMeta tipoDefinicao,
            decimal? percentualReceita,
            decimal? valorMeta)
        {
            switch (tipoDefinicao)
            {
                case TipoDefinicaoMeta.Percentual when percentualReceita.HasValue && !valorMeta.HasValue:
                    DefinirPorPercentual(percentualReceita.Value);
                    break;
                case TipoDefinicaoMeta.Valor when valorMeta.HasValue && !percentualReceita.HasValue:
                    DefinirPorValor(valorMeta.Value);
                    break;
                default:
                    throw new ArgumentException("A definição da meta deve informar somente o campo compatível com a modalidade.");
            }
        }
    }
}
