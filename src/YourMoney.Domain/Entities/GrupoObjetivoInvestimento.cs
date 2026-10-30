namespace YourMoney.Domain.Entities;

public class GrupoObjetivoInvestimento : BaseEntity
{
    public const int NomeMaximo = 100;
    public const int DescricaoMaxima = 160;
    public const int ImagemUrlMaxima = 500;

    public string Nome { get; private set; } = string.Empty;
    public string NomeNormalizado { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public string? ImagemUrl { get; private set; }
    public decimal? ValorAlvo { get; private set; }
    public DateTime? DataAlvo { get; private set; }
    public Guid? GrupoPaiId { get; private set; }
    public virtual GrupoObjetivoInvestimento? GrupoPai { get; private set; }
    public virtual ICollection<GrupoObjetivoInvestimento> Subobjetivos { get; private set; } = new List<GrupoObjetivoInvestimento>();
    public bool Ativo { get; private set; } = true;
    public DateTime? ArquivadoEm { get; private set; }
    public byte[] Versao { get; private set; } = Array.Empty<byte>();
    public virtual ICollection<Investimento> Investimentos { get; private set; } = new List<Investimento>();
    public virtual ICollection<MetaMensal> MetasMensais { get; private set; } = new List<MetaMensal>();
    public virtual ICollection<AporteObjetivoInvestimento> Aportes { get; private set; } = new List<AporteObjetivoInvestimento>();

    private GrupoObjetivoInvestimento() { }

    public GrupoObjetivoInvestimento(
        string nome,
        string usuarioId,
        decimal? valorAlvo = null,
        DateTime? dataAlvo = null,
        string? descricao = null,
        string? imagemUrl = null,
        Guid? grupoPaiId = null)
    {
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        Renomear(nome);
        DefinirPlanejamento(valorAlvo, dataAlvo);
        DefinirApresentacao(descricao, imagemUrl);
        DefinirGrupoPai(grupoPaiId);
        Ativo = true;
    }

    public void Atualizar(string nome, decimal? valorAlvo, DateTime? dataAlvo, string? descricao = null, string? imagemUrl = null, Guid? grupoPaiId = null)
    {
        Renomear(nome);
        DefinirPlanejamento(valorAlvo, dataAlvo);
        DefinirApresentacao(descricao, imagemUrl);
        DefinirGrupoPai(grupoPaiId);
    }

    public void DefinirGrupoPai(Guid? grupoPaiId) => GrupoPaiId = grupoPaiId;

    public void DefinirApresentacao(string? descricao, string? imagemUrl)
    {
        var descricaoLimpa = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        if (descricaoLimpa?.Length > DescricaoMaxima)
            throw new ArgumentException($"A descrição deve ter no máximo {DescricaoMaxima} caracteres.", nameof(descricao));

        var imagemLimpa = string.IsNullOrWhiteSpace(imagemUrl) ? null : imagemUrl.Trim();
        if (imagemLimpa?.Length > ImagemUrlMaxima)
            throw new ArgumentException($"A URL da imagem deve ter no máximo {ImagemUrlMaxima} caracteres.", nameof(imagemUrl));
        if (imagemLimpa != null && !imagemLimpa.StartsWith('/'))
            throw new ArgumentException("A imagem do objetivo deve ser um caminho interno válido.", nameof(imagemUrl));

        Descricao = descricaoLimpa;
        ImagemUrl = imagemLimpa;
    }

    public void DefinirPlanejamento(decimal? valorAlvo, DateTime? dataAlvo)
    {
        if (valorAlvo.HasValue && (valorAlvo <= 0 || valorAlvo != decimal.Round(valorAlvo.Value, 2, MidpointRounding.AwayFromZero)))
            throw new ArgumentException("Valor-alvo deve ser positivo e ter no máximo duas casas decimais.", nameof(valorAlvo));
        ValorAlvo = valorAlvo;
        DataAlvo = dataAlvo?.Date;
    }

    public void Arquivar()
    {
        Ativo = false;
        ArquivadoEm ??= DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        ArquivadoEm = null;
    }

    public void Renomear(string nome)
    {
        var nomeLimpo = nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nomeLimpo))
            throw new ArgumentException("Nome do objetivo financeiro é obrigatório.", nameof(nome));
        if (nomeLimpo.Length > NomeMaximo)
            throw new ArgumentException($"Nome do objetivo financeiro deve ter no máximo {NomeMaximo} caracteres.", nameof(nome));

        Nome = nomeLimpo;
        NomeNormalizado = NormalizarNome(nomeLimpo);
    }

    public static string NormalizarNome(string nome) =>
        (nome ?? string.Empty).Trim().ToUpperInvariant();
}
