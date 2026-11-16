namespace YourMoney.Domain.Entities;

public sealed class FaturaConta : BaseEntity
{
    public Guid IdContaFinanceira { get; private set; }
    public ContaFinanceira ContaFinanceira { get; private set; }
    public DateTime MesReferencia { get; private set; }
    public decimal ValorInformado { get; private set; }

    private FaturaConta() { }

    public FaturaConta(Guid contaId, DateTime mesReferencia, decimal valorInformado, string usuarioId)
    {
        if (contaId == Guid.Empty) throw new ArgumentException("Conta é obrigatória.", nameof(contaId));
        Id = Guid.NewGuid();
        IdContaFinanceira = contaId;
        MesReferencia = new DateTime(mesReferencia.Year, mesReferencia.Month, 1);
        DefinirUsuario(usuarioId);
        AtualizarValor(valorInformado);
    }

    public void AtualizarValor(decimal valorInformado)
    {
        if (valorInformado < 0 || decimal.Round(valorInformado, 2) != valorInformado)
            throw new ArgumentException("O valor da fatura deve ser não negativo e ter até dois centavos.", nameof(valorInformado));
        ValorInformado = valorInformado;
    }
}
