namespace YourMoney.Application.DTOs
{
    public class DespesaRecorrenteRequest
    {
        public string? Descricao { get; set; }
        public decimal ValorPrevisto { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public Guid IdTipoDespesa { get; set; }
        public Guid IdNaturezaDespesa { get; set; }
        public Guid IdCategoria { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataTermino { get; set; }
    }

    public class DespesaRecorrenteResponse
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorPrevisto { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public string ContaDescricao { get; set; } = string.Empty;
        public Guid IdTipoDespesa { get; set; }
        public string TipoDescricao { get; set; } = string.Empty;
        public Guid IdNaturezaDespesa { get; set; }
        public string NaturezaDescricao { get; set; } = string.Empty;
        public Guid IdCategoria { get; set; }
        public string CategoriaDescricao { get; set; } = string.Empty;
        public int DiaVencimento { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataTermino { get; set; }
        public bool Ativa { get; set; }
    }

    public class ListarDespesasRecorrentesResponse
    {
        public List<DespesaRecorrenteResponse> Itens { get; set; } = new();
    }

    public class EncerrarDespesaRecorrenteRequest
    {
        public DateTime DataTermino { get; set; }
    }

    public class SugestaoDespesaRecorrenteResponse
    {
        public Guid OcorrenciaId { get; set; }
        public Guid DespesaRecorrenteId { get; set; }
        public DateTime MesReferencia { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorPrevisto { get; set; }
        public DateTime DataSugerida { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public string ContaDescricao { get; set; } = string.Empty;
        public Guid IdTipoDespesa { get; set; }
        public Guid IdNaturezaDespesa { get; set; }
        public Guid IdCategoria { get; set; }
        public Guid? DespesaConfirmadaId { get; set; }
    }

    public class ListarSugestoesDespesasRecorrentesResponse
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public List<SugestaoDespesaRecorrenteResponse> Itens { get; set; } = new();
    }

    public class ConfirmarSugestaoDespesaRecorrenteRequest
    {
        public string? Descricao { get; set; }
        public decimal? Valor { get; set; }
        public DateTime? Data { get; set; }
        public Guid? IdContaFinanceira { get; set; }
        public Guid? IdTipoDespesa { get; set; }
        public Guid? IdNaturezaDespesa { get; set; }
        public Guid? IdCategoria { get; set; }
    }
}
