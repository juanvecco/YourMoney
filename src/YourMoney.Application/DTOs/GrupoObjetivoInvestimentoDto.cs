namespace YourMoney.Application.DTOs;

public sealed class CriarGrupoObjetivoRequest
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public string? ImagemUrl { get; set; }
    public decimal? ValorAlvo { get; set; }
    public DateTime? DataAlvo { get; set; }
    public Guid? GrupoPaiId { get; set; }
}

public sealed class RenomearGrupoObjetivoRequest
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public string? ImagemUrl { get; set; }
    public string? Versao { get; set; }
    public decimal? ValorAlvo { get; set; }
    public DateTime? DataAlvo { get; set; }
    public Guid? GrupoPaiId { get; set; }
}

public sealed class ExcluirGrupoObjetivoRequest
{
    public string? VersaoOrigem { get; set; }
    public string? AcaoVinculos { get; set; }
    public Guid? GrupoDestinoId { get; set; }
    public string? VersaoDestino { get; set; }
}

public class GrupoObjetivoInvestimentoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? ImagemUrl { get; set; }
    public string Versao { get; set; } = string.Empty;
    public int QuantidadeInvestimentos { get; set; }
    public decimal? ValorAlvo { get; set; }
    public DateTime? DataAlvo { get; set; }
    public Guid? GrupoPaiId { get; set; }
    public bool Ativo { get; set; }
    public DateTime? ArquivadoEm { get; set; }
}

public sealed class VersaoGrupoObjetivoRequest { public string? Versao { get; set; } }

public sealed class GrupoObjetivoInvestimentoResumoResponse : GrupoObjetivoInvestimentoResponse
{
    public decimal ValorAtualCarteira { get; set; }
    public decimal? PercentualAlvoAtingido { get; set; }
    public decimal ValorPlanejadoMes { get; set; }
    public decimal ValorConfirmadoMes { get; set; }
    public decimal DiferencaMes { get; set; }
    public string StatusPlanejamento { get; set; } = "AbaixoPlanejado";
    public decimal ValorAguardandoAlocacao { get; set; }
    public List<InvestimentoObjetivoResumoDTO> Investimentos { get; set; } = new();
    public List<AporteObjetivoInvestimentoDTO> Aportes { get; set; } = new();
}

public sealed class InvestimentoObjetivoResumoDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal ValorAtual { get; set; }
}

public sealed class ErroApiResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int? Status { get; set; }
}
