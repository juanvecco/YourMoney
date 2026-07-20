using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class DespesaRecorrente : BaseEntity
    {
        public string Descricao { get; private set; }
        public decimal ValorPrevisto { get; private set; }
        public Guid IdContaFinanceira { get; private set; }
        public virtual ContaFinanceira ContaFinanceira { get; private set; }
        public Guid IdTipoDespesa { get; private set; }
        public virtual Categoria TipoDespesa { get; private set; }
        public Guid IdNaturezaDespesa { get; private set; }
        public virtual Categoria NaturezaDespesa { get; private set; }
        public Guid IdCategoria { get; private set; }
        public virtual Categoria Categoria { get; private set; }
        public int DiaVencimento { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataTermino { get; private set; }
        public bool Ativa { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }
        public virtual ICollection<DespesaRecorrenteOcorrencia> Ocorrencias { get; private set; } = new List<DespesaRecorrenteOcorrencia>();

        private DespesaRecorrente() { }

        public DespesaRecorrente(
            string descricao,
            decimal valorPrevisto,
            Guid idContaFinanceira,
            Guid idTipoDespesa,
            Guid idNaturezaDespesa,
            Guid idCategoria,
            DateTime dataVencimento,
            DateTime dataInicio,
            DateTime? dataTermino,
            string usuarioId)
        {
            Id = Guid.NewGuid();
            CriadoEm = DateTime.UtcNow;
            Ativa = true;
            DefinirUsuario(usuarioId);
            Atualizar(descricao, valorPrevisto, idContaFinanceira, idTipoDespesa, idNaturezaDespesa, idCategoria, dataVencimento, dataInicio, dataTermino);
        }

        public void Atualizar(
            string descricao,
            decimal valorPrevisto,
            Guid idContaFinanceira,
            Guid idTipoDespesa,
            Guid idNaturezaDespesa,
            Guid idCategoria,
            DateTime dataVencimento,
            DateTime dataInicio,
            DateTime? dataTermino)
        {
            AtualizarDescricao(descricao);
            AtualizarValorPrevisto(valorPrevisto);
            AtualizarContaFinanceira(idContaFinanceira);
            AtualizarCategorias(idTipoDespesa, idNaturezaDespesa, idCategoria);
            AtualizarPeriodo(dataVencimento.Day, dataInicio, dataTermino);
            AtualizadoEm = DateTime.UtcNow;
        }

        public void Desativar()
        {
            Ativa = false;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void Encerrar(DateTime dataTermino)
        {
            if (dataTermino.Date < DataInicio.Date)
                throw new ArgumentException("Data de término não pode ser anterior à data de início.", nameof(dataTermino));

            DataTermino = dataTermino.Date;
            AtualizadoEm = DateTime.UtcNow;
        }

        public bool EstaElegivelParaMes(DateTime mesReferencia)
        {
            var mes = NormalizarMesReferencia(mesReferencia);
            var inicio = NormalizarMesReferencia(DataInicio);
            var termino = DataTermino.HasValue ? NormalizarMesReferencia(DataTermino.Value) : (DateTime?)null;
            return Ativa && mes >= inicio && (!termino.HasValue || mes <= termino.Value);
        }

        public DateTime CalcularDataSugerida(DateTime mesReferencia)
        {
            var mes = NormalizarMesReferencia(mesReferencia);
            var dia = Math.Min(DiaVencimento, DateTime.DaysInMonth(mes.Year, mes.Month));
            return new DateTime(mes.Year, mes.Month, dia);
        }

        public static DateTime NormalizarMesReferencia(DateTime data)
        {
            if (data == default)
                throw new ArgumentException("Mês de referência é obrigatório.", nameof(data));

            return new DateTime(data.Year, data.Month, 1);
        }

        private void AtualizarDescricao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));
            if (descricao.Trim().Length > 255)
                throw new ArgumentException("Descrição deve ter no máximo 255 caracteres.", nameof(descricao));

            Descricao = descricao.Trim();
        }

        private void AtualizarValorPrevisto(decimal valorPrevisto)
        {
            if (valorPrevisto <= 0)
                throw new ArgumentException("Valor previsto deve ser maior que zero.", nameof(valorPrevisto));

            ValorPrevisto = decimal.Round(valorPrevisto, 2, MidpointRounding.AwayFromZero);
        }

        private void AtualizarContaFinanceira(Guid idContaFinanceira)
        {
            if (idContaFinanceira == Guid.Empty)
                throw new ArgumentException("Conta Financeira é obrigatória.", nameof(idContaFinanceira));

            IdContaFinanceira = idContaFinanceira;
        }

        private void AtualizarCategorias(Guid idTipoDespesa, Guid idNaturezaDespesa, Guid idCategoria)
        {
            if (idTipoDespesa == Guid.Empty)
                throw new ArgumentException("Tipo é obrigatório.", nameof(idTipoDespesa));
            if (idNaturezaDespesa == Guid.Empty)
                throw new ArgumentException("Natureza é obrigatória.", nameof(idNaturezaDespesa));
            if (idCategoria == Guid.Empty)
                throw new ArgumentException("Categoria é obrigatória.", nameof(idCategoria));

            IdTipoDespesa = idTipoDespesa;
            IdNaturezaDespesa = idNaturezaDespesa;
            IdCategoria = idCategoria;
        }

        private void AtualizarPeriodo(int diaVencimento, DateTime dataInicio, DateTime? dataTermino)
        {
            if (diaVencimento < 1 || diaVencimento > 31)
                throw new ArgumentException("Dia de vencimento deve estar entre 1 e 31.", nameof(diaVencimento));
            if (dataInicio == default)
                throw new ArgumentException("Data de início é obrigatória.", nameof(dataInicio));
            if (dataTermino.HasValue && dataTermino.Value.Date < dataInicio.Date)
                throw new ArgumentException("Data de término não pode ser anterior à data de início.", nameof(dataTermino));

            DiaVencimento = diaVencimento;
            DataInicio = dataInicio.Date;
            DataTermino = dataTermino?.Date;
        }
    }
}
