using YourMoney.Domain.Enums;

namespace YourMoney.Domain.Entities
{
    public class ReceitaRecorrenteOcorrencia : BaseEntity
    {
        public Guid ReceitaRecorrenteId { get; private set; }
        public virtual ReceitaRecorrente ReceitaRecorrente { get; private set; } = null!;
        public DateTime MesReferencia { get; private set; }
        public StatusReceitaRecorrenteOcorrencia Status { get; private set; }
        public Guid? ReceitaConfirmadaId { get; private set; }
        public virtual Receita? ReceitaConfirmada { get; private set; }
        public DateTime? FinalizadaEm { get; private set; }
        public DateTime CriadoEm { get; private set; }

        private ReceitaRecorrenteOcorrencia() { }

        public ReceitaRecorrenteOcorrencia(Guid receitaRecorrenteId, DateTime mesReferencia, string usuarioId)
        {
            if (receitaRecorrenteId == Guid.Empty)
                throw new ArgumentException("Receita recorrente é obrigatória.", nameof(receitaRecorrenteId));

            Id = Guid.NewGuid();
            ReceitaRecorrenteId = receitaRecorrenteId;
            MesReferencia = ReceitaRecorrente.NormalizarMesReferencia(mesReferencia);
            Status = StatusReceitaRecorrenteOcorrencia.Pendente;
            CriadoEm = DateTime.UtcNow;
            DefinirUsuario(usuarioId);
        }

        public bool EstaPendente => Status == StatusReceitaRecorrenteOcorrencia.Pendente;

        public void Confirmar(Guid receitaConfirmadaId)
        {
            if (receitaConfirmadaId == Guid.Empty)
                throw new ArgumentException("Receita confirmada é obrigatória.", nameof(receitaConfirmadaId));
            GarantirPendente();

            Status = StatusReceitaRecorrenteOcorrencia.Confirmada;
            ReceitaConfirmadaId = receitaConfirmadaId;
            FinalizadaEm = DateTime.UtcNow;
        }

        public void Ignorar()
        {
            GarantirPendente();
            Status = StatusReceitaRecorrenteOcorrencia.Ignorada;
            FinalizadaEm = DateTime.UtcNow;
        }

        private void GarantirPendente()
        {
            if (!EstaPendente)
                throw new InvalidOperationException("Sugestão mensal já finalizada.");
        }
    }
}
