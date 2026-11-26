using YourMoney.Api.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class PluggyExpenseCategoryResolverTests
{
    public static Task NewCategoryReusesEquivalentSibling()
    {
        var root = new Categoria("Essenciais", TipoTransacao.Receita, "owner-1");
        var pharmacy = new Categoria("Farmácia", TipoTransacao.Receita, "owner-1", root.Id);
        var resolved = PluggyExpenseCategoryResolver.ResolveNew([root, pharmacy], root.Id, "  Farmacia ");

        TestAssert.Equal(pharmacy.Id, resolved.Id,
            "A new category request should reuse an equivalent sibling instead of duplicating it");
        return Task.CompletedTask;
    }

    public static Task NewCategoryInheritsParentTypeAndRejectsInvalidParents()
    {
        var root = new Categoria("Essenciais", TipoTransacao.Receita, "owner-1");
        var nature = new Categoria("Alimentação", TipoTransacao.Receita, "owner-1", root.Id);
        var leaf = new Categoria("Mercado", TipoTransacao.Receita, "owner-1", nature.Id);
        var created = PluggyExpenseCategoryResolver.ResolveNew([root, nature, leaf], nature.Id, "Feira orgânica");

        TestAssert.Equal(nature.Id, created.CategoriaPaiId,
            "A new category should be placed under the parent explicitly selected by the person");
        TestAssert.Equal(nature.TipoTransacao, created.TipoTransacao,
            "A new category should preserve the parent category type");

        return TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(PluggyExpenseCategoryResolver.ResolveNew([root, nature, leaf], leaf.Id, "Quarta camada")),
                "A fourth category level should be rejected")
            .ContinueWith(async _ => await TestAssert.ThrowsAsync<ArgumentException>(
                () => Task.FromResult(PluggyExpenseCategoryResolver.ResolveNew([root], Guid.NewGuid(), "Sem pai")),
                "A parent absent from the authenticated user's categories should be rejected"))
            .Unwrap();
    }

    public static Task ExistingCategoryMustBelongToProvidedOwnerCollection()
    {
        var category = new Categoria("Mercado", TipoTransacao.Receita, "owner-1");
        return TestAssert.ThrowsAsync<ArgumentException>(
            () => Task.FromResult(PluggyExpenseCategoryResolver.ResolveExisting([], category.Id)),
            "A category not returned for the authenticated owner must not be accepted");
    }

    public static Task NewCategoryRejectsAmbiguousExistingSiblings()
    {
        var root = new Categoria("Essenciais", TipoTransacao.Receita, "owner-1");
        var first = new Categoria("Farmácia", TipoTransacao.Receita, "owner-1", root.Id);
        var second = new Categoria("Farmacia", TipoTransacao.Receita, "owner-1", root.Id);
        return TestAssert.ThrowsAsync<ArgumentException>(
            () => Task.FromResult(PluggyExpenseCategoryResolver.ResolveNew([root, first, second], root.Id, "FARMACIA")),
            "Ambiguous sibling categories must not be selected arbitrarily");
    }
}
