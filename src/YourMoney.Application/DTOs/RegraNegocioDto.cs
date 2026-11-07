namespace YourMoney.Application.DTOs;

public sealed record RegraNegocioDto(
    string Codigo,
    string Nome,
    string Descricao,
    string Grupo,
    bool Ativa,
    bool Efetiva,
    IReadOnlyList<string> Dependencias);
