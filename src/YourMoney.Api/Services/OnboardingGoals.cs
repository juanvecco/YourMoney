namespace YourMoney.Api.Services;

public static class OnboardingGoals
{
    public static IReadOnlyList<string>? Resolve(IReadOnlyCollection<string>? selected, string? legacy)
    {
        if (selected is { Count: > 0 })
        {
            if (selected.Count > 4 || selected.Any(goal => goal is not ("gastos" or "cartao" or "objetivos" or "saldo")))
                return null;
            return selected.Distinct(StringComparer.Ordinal).ToArray();
        }

        return legacy is "gastos" or "cartao" or "objetivos" or "saldo" ? [legacy] : null;
    }
}
