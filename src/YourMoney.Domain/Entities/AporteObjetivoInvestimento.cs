namespace YourMoney.Domain.Entities;

public sealed class AporteObjetivoInvestimento : BaseEntity
{
    public Guid MetaMensalId { get; private set; }
    public Guid GrupoObjetivoInvestimentoId { get; private set; }
    public Guid? InvestimentoId { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime MesReferencia { get; private set; }
    public DateTime DataConfirmacao { get; private set; }
    public Guid OperacaoId { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public MetaMensal MetaMensal { get; private set; } = null!;
    public GrupoObjetivoInvestimento GrupoObjetivoInvestimento { get; private set; } = null!;
    public Investimento? Investimento { get; private set; }

    private AporteObjetivoInvestimento() { }

    public AporteObjetivoInvestimento(string usuarioId, Guid metaMensalId, Guid grupoObjetivoInvestimentoId,
        decimal valor, DateTime mesReferencia, DateTime dataConfirmacao, Guid operacaoId, Guid? investimentoId = null)
    {
        if (metaMensalId == Guid.Empty || grupoObjetivoInvestimentoId == Guid.Empty || operacaoId == Guid.Empty)
            throw new ArgumentException("Meta, objetivo e operação são obrigatórios.");
        if (valor <= 0 || valor != decimal.Round(valor, 2, MidpointRounding.AwayFromZero))
            throw new ArgumentException("Valor do aporte deve ser positivo e ter no máximo duas casas decimais.", nameof(valor));
        if (mesReferencia == default || dataConfirmacao == default)
            throw new ArgumentException("Mês de referência e data de confirmação são obrigatórios.");
        if (investimentoId == Guid.Empty)
            throw new ArgumentException("Investimento de destino inválido.", nameof(investimentoId));

        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        MetaMensalId = metaMensalId;
        GrupoObjetivoInvestimentoId = grupoObjetivoInvestimentoId;
        Valor = valor;
        MesReferencia = new DateTime(mesReferencia.Year, mesReferencia.Month, 1);
        DataConfirmacao = dataConfirmacao.Date;
        OperacaoId = operacaoId;
        InvestimentoId = investimentoId;
        CriadoEm = DateTime.UtcNow;
    }

    public void Alocar(Guid investimentoId)
    {
        if (investimentoId == Guid.Empty) throw new ArgumentException("Investimento de destino inválido.");
        if (InvestimentoId.HasValue && InvestimentoId != investimentoId)
            throw new InvalidOperationException("A alocação confirmada não pode ser trocada.");
        InvestimentoId = investimentoId;
        AtualizadoEm = DateTime.UtcNow;
    }

    public bool Corresponde(Guid metaId, Guid grupoId, decimal valor, DateTime mes, DateTime data, Guid? investimentoId) =>
        MetaMensalId == metaId && GrupoObjetivoInvestimentoId == grupoId && Valor == valor
        && MesReferencia == new DateTime(mes.Year, mes.Month, 1) && DataConfirmacao == data.Date
        && InvestimentoId == investimentoId;
}
