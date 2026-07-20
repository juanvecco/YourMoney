using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class DespesaRecorrenteOcorrencia : BaseEntity
    {
        public Guid DespesaRecorrenteId { get; private set; }
        public virtual DespesaRecorrente DespesaRecorrente { get; private set; }
        public DateTime MesReferencia { get; private set; }
        public StatusDespesaRecorrenteOcorrencia Status { get; private set; }
        public Guid? DespesaConfirmadaId { get; private set; }
        public virtual Despesa DespesaConfirmada { get; private set; }
        public DateTime? FinalizadaEm { get; private set; }
        public DateTime CriadoEm { get; private set; }

        private DespesaRecorrenteOcorrencia() { }

        public DespesaRecorrenteOcorrencia(Guid despesaRecorrenteId, DateTime mesReferencia, string usuarioId)
        {
            if (despesaRecorrenteId == Guid.Empty)
                throw new ArgumentException("Despesa recorrente é obrigatória.", nameof(despesaRecorrenteId));

            Id = Guid.NewGuid();
            DespesaRecorrenteId = despesaRecorrenteId;
            MesReferencia = DespesaRecorrente.NormalizarMesReferencia(mesReferencia);
            Status = StatusDespesaRecorrenteOcorrencia.Pendente;
            CriadoEm = DateTime.UtcNow;
            DefinirUsuario(usuarioId);
        }

        public void Confirmar(Guid despesaConfirmadaId)
        {
            if (despesaConfirmadaId == Guid.Empty)
                throw new ArgumentException("Despesa confirmada é obrigatória.", nameof(despesaConfirmadaId));
            GarantirPendente();

            Status = StatusDespesaRecorrenteOcorrencia.Confirmada;
            DespesaConfirmadaId = despesaConfirmadaId;
            FinalizadaEm = DateTime.UtcNow;
        }

        public void Ignorar()
        {
            GarantirPendente();
            Status = StatusDespesaRecorrenteOcorrencia.Ignorada;
            FinalizadaEm = DateTime.UtcNow;
        }

        public bool EstaPendente => Status == StatusDespesaRecorrenteOcorrencia.Pendente;

        private void GarantirPendente()
        {
            if (!EstaPendente)
                throw new InvalidOperationException("Sugestão mensal já finalizada.");
        }
    }
}
