namespace YourMoney.Domain.Entities;

public sealed class RegraNegocioUsuario : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public bool Ativa { get; private set; }
    public DateTimeOffset AtualizadaEm { get; private set; }

    private RegraNegocioUsuario() { }

    public RegraNegocioUsuario(string usuarioId, string codigo, bool ativa)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("O código da regra é obrigatório.", nameof(codigo));
        Id = Guid.NewGuid();
        DefinirUsuario(usuarioId);
        Codigo = codigo;
        Alterar(ativa);
    }

    public void Alterar(bool ativa)
    {
        Ativa = ativa;
        AtualizadaEm = DateTimeOffset.UtcNow;
    }
}
