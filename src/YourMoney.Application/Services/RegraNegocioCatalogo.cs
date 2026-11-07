namespace YourMoney.Application.Services;

public sealed record RegraNegocioDefinicao(
    string Codigo,
    string Nome,
    string Descricao,
    string Grupo,
    IReadOnlyList<string> Dependencias);

public static class RegraNegocioCatalogo
{
    public const string OpenFinance = "openfinance.visivel";
    public const string DespesasRecorrentes = "despesas.recorrencia";
    public const string ReceitasRecorrentes = "receitas.recorrencia";
    public const string Parcelamento = "despesas.parcelamento";
    public const string ReservaEmergencia = "receitas.reserva-emergencia";
    public const string RendaSalarial = "investimentos.renda-salarial";
    public const string MetasMensais = "metas.mensais";
    public const string ObjetivosInvestimento = "investimentos.objetivos";
    public const string AtivosOpenFinance = "investimentos.ativos-openfinance";

    public static IReadOnlyList<RegraNegocioDefinicao> Todas { get; } = new[]
    {
        new RegraNegocioDefinicao(OpenFinance, "Open Finance", "Exibe conexões, saldos e prévias bancárias. Importações continuam a exigir confirmação.", "Open Finance", Array.Empty<string>()),
        new RegraNegocioDefinicao(DespesasRecorrentes, "Recorrência de despesas", "Mostra despesas recorrentes e sugestões mensais para confirmação.", "Despesas", Array.Empty<string>()),
        new RegraNegocioDefinicao(ReceitasRecorrentes, "Recorrência de receitas", "Mostra receitas recorrentes e sugestões mensais para confirmação.", "Receitas", Array.Empty<string>()),
        new RegraNegocioDefinicao(Parcelamento, "Parcelamento de despesas", "Permite distribuir uma nova despesa em parcelas.", "Despesas", Array.Empty<string>()),
        new RegraNegocioDefinicao(ReservaEmergencia, "Reserva de emergência", "Exibe projeções de reserva com base nas receitas recorrentes marcadas.", "Planejamento", new[] { ReceitasRecorrentes }),
        new RegraNegocioDefinicao(RendaSalarial, "Investimentos por renda salarial", "Permite associar investimentos a uma receita recorrente marcada como salário.", "Investimentos", new[] { ReceitasRecorrentes }),
        new RegraNegocioDefinicao(MetasMensais, "Metas mensais", "Mostra planejamento mensal, metas e alertas.", "Planejamento", Array.Empty<string>()),
        new RegraNegocioDefinicao(ObjetivosInvestimento, "Objetivos de investimento", "Mostra objetivos, aportes e seu progresso.", "Investimentos", Array.Empty<string>()),
        new RegraNegocioDefinicao(AtivosOpenFinance, "Ativos Open Finance", "Mostra ativos bancários na tela de investimentos e permite conciliação manual.", "Investimentos", new[] { OpenFinance })
    };

    public static RegraNegocioDefinicao? Obter(string codigo) =>
        Todas.FirstOrDefault(rule => string.Equals(rule.Codigo, codigo, StringComparison.Ordinal));
}
