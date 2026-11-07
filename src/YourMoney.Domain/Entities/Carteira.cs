using System.Globalization;
using System.Text;

namespace YourMoney.Domain.Entities;

public sealed class Carteira : BaseEntity
{
    public const int NomeMaximo = 80;

    public string Nome { get; private set; } = string.Empty;
    public string NomeNormalizado { get; private set; } = string.Empty;
    public bool Principal { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }
    public DateTimeOffset? AtualizadaEm { get; private set; }

    private Carteira()
    {
    }

    public Carteira(string usuarioId, string nome, bool principal = false)
    {
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        DefinirNome(nome);
        Principal = principal;
        CriadaEm = DateTimeOffset.UtcNow;
    }

    public void Renomear(string nome)
    {
        DefinirNome(nome);
        AtualizadaEm = DateTimeOffset.UtcNow;
    }

    private void DefinirNome(string nome)
    {
        var normalizado = string.Join(
            ' ',
            (nome ?? string.Empty).Normalize(NormalizationForm.FormKC)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalizado.Length is < 1 or > NomeMaximo)
            throw new ArgumentException($"O nome da carteira deve ter entre 1 e {NomeMaximo} caracteres.", nameof(nome));

        Nome = normalizado;
        NomeNormalizado = normalizado.ToUpper(CultureInfo.InvariantCulture);
    }
}
