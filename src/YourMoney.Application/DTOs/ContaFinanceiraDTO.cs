namespace YourMoney.Application.DTOs;

public sealed class ContaFinanceiraRequest
{
    public string Descricao { get; set; } = string.Empty;
}

public sealed class ContaFinanceiraResponse
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public bool PodeRemover { get; set; }
}

public sealed class ContaFinanceiraEmUsoException : InvalidOperationException
{
    public ContaFinanceiraEmUsoException()
        : base("Esta conta possui movimentações ou recorrências vinculadas e não pode ser removida.")
    {
    }
}
