namespace YourMoney.Application.DTOs;

public sealed class ConfirmarAporteObjetivoRequest
{
    public decimal Valor { get; set; }
    public DateTime DataConfirmacao { get; set; }
    public Guid? InvestimentoId { get; set; }
    public Guid OperacaoId { get; set; }
}

public sealed class AlocarAporteObjetivoRequest { public Guid InvestimentoId { get; set; } }

public sealed class AporteObjetivoInvestimentoDTO
{
    public Guid Id { get; set; }
    public Guid MetaMensalId { get; set; }
    public Guid GrupoObjetivoInvestimentoId { get; set; }
    public Guid? InvestimentoId { get; set; }
    public string? InvestimentoNome { get; set; }
    public decimal Valor { get; set; }
    public DateTime MesReferencia { get; set; }
    public DateTime DataConfirmacao { get; set; }
    public bool AguardandoAlocacao => !InvestimentoId.HasValue;
    public bool CriadoAgora { get; set; }
}
