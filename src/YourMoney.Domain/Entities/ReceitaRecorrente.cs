using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class ReceitaRecorrente : BaseEntity
    {
        public string Descricao { get; private set; } = string.Empty;
        public decimal ValorPrevisto { get; private set; }
        public Guid IdContaFinanceira { get; private set; }
        public virtual ContaFinanceira? ContaFinanceira { get; private set; }
        public NaturezaReceita Natureza { get; private set; }
        public bool EhSalario { get; private set; }
        public bool ConsideraReservaEmergencia { get; private set; }
        public int DiaRecebimento { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataTermino { get; private set; }
        public bool Ativa { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }
        public virtual ICollection<ReceitaRecorrenteOcorrencia> Ocorrencias { get; private set; } = new List<ReceitaRecorrenteOcorrencia>();

        private ReceitaRecorrente() { }

        public ReceitaRecorrente(
            string descricao,
            decimal valorPrevisto,
            Guid idContaFinanceira,
            NaturezaReceita natureza,
            bool ehSalario,
            bool consideraReservaEmergencia,
            DateTime dataRecebimento,
            DateTime dataInicio,
            DateTime? dataTermino,
            string usuarioId)
        {
            Id = Guid.NewGuid();
            CriadoEm = DateTime.UtcNow;
            Ativa = true;
            DefinirUsuario(usuarioId);
            Atualizar(
                descricao,
                valorPrevisto,
                idContaFinanceira,
                natureza,
                ehSalario,
                consideraReservaEmergencia,
                dataRecebimento,
                dataInicio,
                dataTermino);
        }

        public void Atualizar(
            string descricao,
            decimal valorPrevisto,
            Guid idContaFinanceira,
            NaturezaReceita natureza,
            bool ehSalario,
            bool consideraReservaEmergencia,
            DateTime dataRecebimento,
            DateTime dataInicio,
            DateTime? dataTermino)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));
            if (descricao.Trim().Length > 255)
                throw new ArgumentException("Descrição deve ter no máximo 255 caracteres.", nameof(descricao));
            if (valorPrevisto <= 0)
                throw new ArgumentException("Valor previsto deve ser maior que zero.", nameof(valorPrevisto));
            if (idContaFinanceira == Guid.Empty)
                throw new ArgumentException("Conta Financeira é obrigatória.", nameof(idContaFinanceira));
            if (!Enum.IsDefined(typeof(NaturezaReceita), natureza) || natureza == NaturezaReceita.Reembolso)
                throw new ArgumentException("Natureza da receita recorrente é inválida.", nameof(natureza));
            if (dataRecebimento == default)
                throw new ArgumentException("Data de recebimento é obrigatória.", nameof(dataRecebimento));
            if (dataInicio == default)
                throw new ArgumentException("Data de início é obrigatória.", nameof(dataInicio));
            if (dataTermino.HasValue && dataTermino.Value.Date < dataInicio.Date)
                throw new ArgumentException("Data de término não pode ser anterior à data de início.", nameof(dataTermino));

            Descricao = descricao.Trim();
            ValorPrevisto = decimal.Round(valorPrevisto, 2, MidpointRounding.AwayFromZero);
            IdContaFinanceira = idContaFinanceira;
            Natureza = natureza;
            EhSalario = ehSalario;
            ConsideraReservaEmergencia = consideraReservaEmergencia;
            DiaRecebimento = dataRecebimento.Day;
            DataInicio = dataInicio.Date;
            DataTermino = dataTermino?.Date;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void Desativar()
        {
            Ativa = false;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void Encerrar(DateTime dataTermino)
        {
            if (dataTermino == default)
                throw new ArgumentException("Data de término é obrigatória.", nameof(dataTermino));
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

        public bool EstaElegivelParaReserva(DateTime mesReferencia)
        {
            return ConsideraReservaEmergencia && EstaElegivelParaMes(mesReferencia);
        }

        public DateTime CalcularDataSugerida(DateTime mesReferencia)
        {
            var mes = NormalizarMesReferencia(mesReferencia);
            var dia = Math.Min(DiaRecebimento, DateTime.DaysInMonth(mes.Year, mes.Month));
            return new DateTime(mes.Year, mes.Month, dia);
        }

        public static DateTime NormalizarMesReferencia(DateTime data)
        {
            if (data == default)
                throw new ArgumentException("Mês de referência é obrigatório.", nameof(data));

            return new DateTime(data.Year, data.Month, 1);
        }
    }
}
