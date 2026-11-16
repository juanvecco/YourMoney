namespace YourMoney.Application.DTOs;

public sealed class SalvarFaturaRequest
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal ValorInformado { get; set; }
}

public sealed class ResumoFaturaResponse
{
    public Guid IdContaFinanceira { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal? ValorInformado { get; set; }
    public decimal TotalCadastrado { get; set; }
    public decimal? Diferenca { get; set; }
    public int QuantidadeDespesas { get; set; }
    public int QuantidadePendentes { get; set; }
}
