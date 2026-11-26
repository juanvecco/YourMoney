using System.Globalization;
using System.Text;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;

namespace YourMoney.Api.Services;

public static class PluggyExpenseCategoryResolver
{
    public static Categoria ResolveExisting(IReadOnlyCollection<Categoria> categories, Guid categoryId)
    {
        var category = categories.SingleOrDefault(item => item.Id == categoryId);
        if (category is null || !IsExpenseCategory(category))
            throw new ArgumentException("Selecione uma categoria de despesa pertencente ao seu usuário.");

        return category;
    }

    public static Categoria ResolveNew(
        IReadOnlyCollection<Categoria> categories,
        Guid parentId,
        string description)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 100)
            throw new ArgumentException("Informe um nome de categoria com até 100 caracteres.");

        var parent = categories.SingleOrDefault(item => item.Id == parentId);
        if (parent is null || !IsExpenseCategory(parent))
            throw new ArgumentException("Selecione uma categoria-pai de despesa pertencente ao seu usuário.");
        if (ObterProfundidade(categories, parent) > 1)
            throw new ArgumentException("A categoria-pai deve ser um tipo ou uma natureza de despesa.");

        var normalizedDescription = Normalizar(description);
        var existingSiblings = categories.Where(item =>
            item.CategoriaPaiId == parent.Id
            && string.Equals(Normalizar(item.Descricao), normalizedDescription, StringComparison.Ordinal))
            .ToArray();
        if (existingSiblings.Length > 1)
            throw new ArgumentException("Há mais de uma categoria correspondente. Escolha uma categoria existente antes de salvar.");
        if (existingSiblings.Length == 1)
            return existingSiblings[0];

        return new Categoria(description.Trim(), parent.TipoTransacao, parent.Id);
    }

    private static bool IsExpenseCategory(Categoria category) =>
        category.TipoTransacao is TipoTransacao.Receita or TipoTransacao.Despesa;

    private static int ObterProfundidade(IReadOnlyCollection<Categoria> categories, Categoria category)
    {
        var depth = 0;
        var current = category;
        var visited = new HashSet<Guid> { category.Id };
        while (current.CategoriaPaiId.HasValue)
        {
            if (!visited.Add(current.CategoriaPaiId.Value))
                throw new ArgumentException("A hierarquia da categoria-pai é inválida.");

            current = categories.SingleOrDefault(item => item.Id == current.CategoriaPaiId.Value)
                ?? throw new ArgumentException("A categoria-pai não pertence à hierarquia do seu usuário.");
            depth++;
            if (depth > categories.Count)
                throw new ArgumentException("A hierarquia da categoria-pai é inválida.");
        }

        return depth;
    }

    private static string Normalizar(string value)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(character))
                result.Append(char.ToLowerInvariant(character));
        }

        return result.ToString();
    }
}
