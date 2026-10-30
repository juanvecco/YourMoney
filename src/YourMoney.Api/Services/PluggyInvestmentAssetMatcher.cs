#nullable enable

using System.Globalization;
using System.Text;
using YourMoney.Domain.Entities;

namespace YourMoney.Api.Services;

public sealed record PluggyInvestmentAssetMatch(
    Investimento Investment,
    string Reason,
    decimal? ValueDifference,
    decimal? ValueDifferencePercent);

/// <summary>Suggestions for review; a similar balance never identifies an asset on its own.</summary>
public static class PluggyInvestmentAssetMatcher
{
    private static readonly HashSet<string> IgnoredWords = new(StringComparer.Ordinal)
    {
        "DE", "DA", "DO", "DAS", "DOS", "E", "A", "O", "AS", "OS", "EM", "PARA",
        "THE", "OF", "AND", "BANCO", "BANK", "SA", "S", "LTDA"
    };

    private static readonly HashSet<string> GenericWords = new(StringComparer.Ordinal)
    {
        "CDB", "RDB", "LCI", "LCA", "FUNDO", "FUND", "FIC", "FI", "FIM", "FIA", "RF",
        "RENDA", "FIXA", "INVESTIMENTO", "INVESTIMENTOS", "APLICACAO", "APLICACOES",
        "POS", "PRE", "CDI", "DI", "PREFIXADO", "POSFIXADO", "RESERVA"
    };

    public static IReadOnlyList<PluggyInvestmentAssetMatch> FindCandidates(
        PluggyInvestmentAssetSnapshot asset, IEnumerable<Investimento> investments) => investments
        .Where(investment => investment.UsuarioId == asset.UsuarioId && investment.Ativo)
        .Select(investment => Match(asset, investment))
        .OfType<RankedMatch>()
        .OrderByDescending(match => match.Strength)
        .ThenBy(match => match.Result.ValueDifference.HasValue
            ? Math.Abs(match.Result.ValueDifference.Value) : decimal.MaxValue)
        .ThenByDescending(match => match.Similarity)
        .ThenBy(match => match.Result.Investment.Nome, StringComparer.OrdinalIgnoreCase)
        .ThenBy(match => match.Result.Investment.Id)
        .Take(5)
        .Select(match => match.Result)
        .ToArray();

    private static RankedMatch? Match(PluggyInvestmentAssetSnapshot asset, Investimento investment)
    {
        var exactCode = SameIdentifier(asset.Code, investment.CodigoAtivo);
        var exactIsin = SameIdentifier(asset.Isin, investment.Isin);
        var strength = 3;
        var similarity = 1m;
        string reason;
        if (ConflictingIdentifier(asset.Isin, investment.Isin)
            || (!exactIsin && ConflictingIdentifier(asset.Code, investment.CodigoAtivo)))
            return null;
        if (exactIsin) reason = "ISIN igual";
        else if (exactCode) reason = "Código do ativo igual";
        else
        {
            var source = Normalize(asset.Name);
            var name = Normalize(investment.Nome);
            var description = Normalize(investment.Descricao);
            // A generic description must not hide a different maturity in the manual name.
            var sourceNumbers = Numbers(Words(source));
            var nameNumbers = Numbers(Words(name));
            var localNumbers = nameNumbers.Length > 0 ? nameNumbers : Numbers(Words(description));
            if (sourceNumbers.Length > 0 && localNumbers.Length > 0
                && !sourceNumbers.Intersect(localNumbers, StringComparer.Ordinal).Any()) return null;
            strength = 2;
            if (source.Length > 0 && Key(source) == Key(name)) reason = "Nome igual";
            else if (source.Length > 0 && Key(source) == Key(description)) reason = "Descrição igual";
            else
            {
                var nameSimilarity = TextSimilarity(source, name);
                var descriptionSimilarity = TextSimilarity(source, description);
                similarity = Math.Max(nameSimilarity, descriptionSimilarity);
                if (similarity < .75m) return null;
                strength = 1;
                reason = descriptionSimilarity > nameSimilarity
                    ? "Descrição semelhante; confirme o ativo"
                    : "Nome semelhante; confirme o ativo";
            }
        }

        // Local investments are denominated in BRL; never compare them to a foreign balance.
        decimal? difference = asset.CurrencyCode == "BRL"
            ? decimal.Round(asset.Balance - investment.ValorAtual, 2, MidpointRounding.AwayFromZero) : null;
        decimal? percent = difference.HasValue && investment.ValorAtual > 0
            ? decimal.Round(difference.Value / investment.ValorAtual * 100m, 2, MidpointRounding.AwayFromZero) : null;
        return new RankedMatch(new(investment, reason, difference, percent), strength, similarity);
    }

    private static bool SameIdentifier(string? source, string? local) =>
        !string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(local)
        && string.Equals(source.Trim(), local.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool ConflictingIdentifier(string? source, string? local) =>
        !string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(local) && !SameIdentifier(source, local);

    private static string Normalize(string? value) => new(
        (value ?? string.Empty).Normalize(NormalizationForm.FormD)
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Select(character => char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : ' ')
            .ToArray());

    private static string Key(string value) => value.Replace(" ", string.Empty, StringComparison.Ordinal);

    private static decimal TextSimilarity(string source, string local)
    {
        var sourceWords = Words(source);
        var localWords = Words(local);
        var common = sourceWords.Intersect(localWords, StringComparer.Ordinal).ToArray();
        if (common.Length < 2 || !common.Any(word => word.Length >= 3
            && !GenericWords.Contains(word) && !word.All(char.IsDigit))) return 0;

        var sourceNumbers = Numbers(sourceWords);
        var localNumbers = Numbers(localWords);
        if (sourceNumbers.Length > 0 && localNumbers.Length > 0
            && !sourceNumbers.Intersect(localNumbers, StringComparer.Ordinal).Any()) return 0;

        return 2m * common.Length / (sourceWords.Length + localWords.Length);
    }

    private static string[] Words(string value) => value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Where(word => !IgnoredWords.Contains(word)).Distinct(StringComparer.Ordinal).ToArray();

    private static string[] Numbers(IEnumerable<string> words) =>
        words.Where(word => word.Length >= 4 && word.All(char.IsDigit)).ToArray();

    private sealed record RankedMatch(PluggyInvestmentAssetMatch Result, int Strength, decimal Similarity);
}
