namespace YourMoney.Application.DTOs;

public sealed class ContaFinanceiraRequest
{
    public string Descricao { get; set; } = string.Empty;
    public Guid? CarteiraId { get; set; }
}

public sealed class MoverContaFinanceiraRequest
{
    public Guid CarteiraId { get; set; }
    public bool ConfirmarTransferencia { get; set; }
}

public sealed class ContaFinanceiraResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public bool PodeRemover { get; set; }
    public Guid CarteiraId { get; set; }
}

public sealed class ContaFinanceiraMoverConfirmacaoException : InvalidOperationException
{
    public ContaFinanceiraMoverConfirmacaoException()
        : base("Confirme a transferência desta conta para outra carteira. Os lançamentos vinculados serão preservados.")
    {
    }
}

public sealed class ContaFinanceiraEmUsoException : InvalidOperationException
{
    public ContaFinanceiraEmUsoException()
        : base("Esta conta possui movimentações ou recorrências vinculadas e não pode ser removida.")
    {
    }
}
