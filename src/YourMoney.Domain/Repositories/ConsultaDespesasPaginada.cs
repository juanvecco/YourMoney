using YourMoney.Domain.Entities;

namespace YourMoney.Domain.Repositories;

public sealed record TotalDespesasPorConta(Guid IdContaFinanceira, int Quantidade, decimal ValorLiquido);

public sealed record ConsultaDespesasPaginada(
    IReadOnlyList<Despesa> Itens,
    int PaginaAtual,
    int TotalResultados,
    IReadOnlyList<TotalDespesasPorConta> TotaisPorConta);
