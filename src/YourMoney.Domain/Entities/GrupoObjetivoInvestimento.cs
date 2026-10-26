namespace YourMoney.Domain.Entities;

public class GrupoObjetivoInvestimento : BaseEntity
{
    public const int NomeMaximo = 100;

    public string Nome { get; private set; } = string.Empty;
    public string NomeNormalizado { get; private set; } = string.Empty;
    public byte[] Versao { get; private set; } = Array.Empty<byte>();
    public virtual ICollection<Investimento> Investimentos { get; private set; } = new List<Investimento>();

    private GrupoObjetivoInvestimento() { }

    public GrupoObjetivoInvestimento(string nome, string usuarioId)
    {
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        Renomear(nome);
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
