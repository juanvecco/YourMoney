namespace YourMoney.Application.DTOs
{
    public class ReceitaRecorrenteRequest
    {
        public string? Descricao { get; set; }
        public decimal ValorPrevisto { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public string? Natureza { get; set; }
        public bool EhSalario { get; set; }
        public bool ConsideraReservaEmergencia { get; set; }
        public DateTime DataRecebimento { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataTermino { get; set; }
    }

    public class ReceitaRecorrenteResponse
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorPrevisto { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public string ContaDescricao { get; set; } = string.Empty;
        public string Natureza { get; set; } = string.Empty;
        public bool EhSalario { get; set; }
        public bool ConsideraReservaEmergencia { get; set; }
        public int DiaRecebimento { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataTermino { get; set; }
        public bool Ativa { get; set; }
    }

    public class ListarReceitasRecorrentesResponse
    {
        public List<ReceitaRecorrenteResponse> Itens { get; set; } = new();
    }

    public class EncerrarReceitaRecorrenteRequest
    {
        public DateTime DataTermino { get; set; }
    }

    public class SugestaoReceitaRecorrenteResponse
    {
        public Guid OcorrenciaId { get; set; }
        public Guid ReceitaRecorrenteId { get; set; }
        public DateTime MesReferencia { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorPrevisto { get; set; }
        public DateTime DataSugerida { get; set; }
        public Guid IdContaFinanceira { get; set; }
        public string ContaDescricao { get; set; } = string.Empty;
        public string Natureza { get; set; } = string.Empty;
        public Guid? ReceitaConfirmadaId { get; set; }
    }

    public class ListarSugestoesReceitasRecorrentesResponse
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public List<SugestaoReceitaRecorrenteResponse> Itens { get; set; } = new();
    }

    public class ConfirmarSugestaoReceitaRecorrenteRequest
    {
        public string? Descricao { get; set; }
        public decimal? Valor { get; set; }
        public DateTime? Data { get; set; }
        public Guid? IdContaFinanceira { get; set; }
        public string? Natureza { get; set; }
    }

    public class ProjecaoReservaEmergenciaItemResponse
    {
        public Guid ReceitaRecorrenteId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string ContaDescricao { get; set; } = string.Empty;
        public bool EhSalario { get; set; }
        public decimal ValorMensal { get; set; }
        public decimal ValorSeisMeses { get; set; }
        public decimal ValorDozeMeses { get; set; }
    }

    public class ProjecaoReservaEmergenciaResponse
    {
        public List<ProjecaoReservaEmergenciaItemResponse> Itens { get; set; } = new();
    }
}
