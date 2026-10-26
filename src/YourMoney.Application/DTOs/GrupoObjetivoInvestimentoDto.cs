namespace YourMoney.Application.DTOs;

public sealed class CriarGrupoObjetivoRequest
{
    public string? Nome { get; set; }
}

public sealed class RenomearGrupoObjetivoRequest
{
    public string? Nome { get; set; }
    public string? Versao { get; set; }
}

public sealed class ExcluirGrupoObjetivoRequest
{
    public string? VersaoOrigem { get; set; }
    public string? AcaoVinculos { get; set; }
    public Guid? GrupoDestinoId { get; set; }
    public string? VersaoDestino { get; set; }
}

public sealed class GrupoObjetivoInvestimentoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Versao { get; set; } = string.Empty;
    public int QuantidadeInvestimentos { get; set; }
}

public sealed class ErroApiResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int? Status { get; set; }
}
