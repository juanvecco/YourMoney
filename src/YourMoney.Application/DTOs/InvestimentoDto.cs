using System.ComponentModel.DataAnnotations;

namespace YourMoney.Application.DTOs
{
    /// <summary>
    /// DTO para representar um investimento
    /// </summary>
    public class InvestimentoDto
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? Tipo { get; set; }
        public decimal Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        //public Decimal ValorInvestido { get; private set; }
        public Decimal ValorAtual { get; set; }
        public DateTime DataInvestimento { get; set; }
        public DateTime? DataResgate { get; set; }
        //public decimal TaxaRetorno { get; private set; }
        public bool Ativo { get; set; }

    }

    /// <summary>
    /// DTO para criar um novo investimento
    /// </summary>
    //public class CriarInvestimentoDto
    //{
    //    [Required(ErrorMessage = "O nome do ativo é obrigatório")]
    //    [StringLength(100, ErrorMessage = "O nome do ativo deve ter no máximo 100 caracteres")]
    //    public string Ativo { get; set; } = string.Empty;

    //    [Required(ErrorMessage = "O tipo do investimento é obrigatório")]
    //    [StringLength(50, ErrorMessage = "O tipo deve ter no máximo 50 caracteres")]
    //    public string Tipo { get; set; }

    //    [Required(ErrorMessage = "A quantidade é obrigatória")]
    //    [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
    //    public decimal Quantidade { get; set; }

    //    [Required(ErrorMessage = "O preço médio é obrigatório")]
    //    [Range(0.01, double.MaxValue, ErrorMessage = "O preço médio deve ser maior que zero")]
    //    public decimal PrecoMedio { get; set; }

    //    [Required(ErrorMessage = "A data de compra é obrigatória")]
    //    public DateTime DataCompra { get; set; }

    //    public int UsuarioId { get; set; }
    //}

    /// <summary>
    /// DTO para atualizar um investimento existente
    /// </summary>
    //public class AtualizarInvestimentoDto
    //{
    //    [StringLength(100, ErrorMessage = "O nome do ativo deve ter no máximo 100 caracteres")]
    //    public string? Ativo { get; set; }

    //    [StringLength(50, ErrorMessage = "O tipo deve ter no máximo 50 caracteres")]
    //    public string? Tipo { get; set; }

    //    [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
    //    public decimal? Quantidade { get; set; }

    //    [Range(0.01, double.MaxValue, ErrorMessage = "O preço médio deve ser maior que zero")]
    //    public decimal? PrecoMedio { get; set; }

    //    public DateTime? DataCompra { get; set; }
    //}

    ///// <summary>
    ///// DTO para resumo dos investimentos
    ///// </summary>
    //public class ResumoInvestimentoDto
    //{
    //    public decimal TotalInvestido { get; set; }
    //    public decimal PatrimonioTotal { get; set; }
    //    public decimal Rentabilidade { get; set; }
    //    public decimal RentabilidadePercentual { get; set; }
    //    public int QuantidadeInvestimentos { get; set; }
    //    public DateTime? DataUltimaAtualizacao { get; set; }
    //}

    ///// <summary>
    ///// DTO para distribuição por categoria
    ///// </summary>
    //public class DistribuicaoCategoriaDto
    //{
    //    public string Categoria { get; set; } = string.Empty;
    //    public decimal Valor { get; set; }
    //    public decimal Percentual { get; set; }
    //    public int QuantidadeAtivos { get; set; }
    //}

    ///// <summary>
    ///// DTO para evolução do patrimônio
    ///// </summary>
    //public class EvolucaoPatrimonioDto
    //{
    //    public DateTime Data { get; set; }
    //    public decimal Valor { get; set; }
    //    public decimal Rentabilidade { get; set; }
    //}

    ///// <summary>
    ///// DTO para performance de um investimento
    ///// </summary>
    //public class PerformanceInvestimentoDto
    //{
    //    public int InvestimentoId { get; set; }
    //    public string Ativo { get; set; } = string.Empty;
    //    public decimal RentabilidadeDiaria { get; set; }
    //    public decimal RentabilidadeMensal { get; set; }
    //    public decimal RentabilidadeAnual { get; set; }
    //    public decimal RentabilidadeTotal { get; set; }
    //    public DateTime DataUltimaAtualizacao { get; set; }
    //}
}

