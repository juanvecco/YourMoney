using YourMoney.Domain.Entities;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Domain;

public static class CarteiraTests
{
    public static async Task TrimsNameAndStoresPrincipalState()
    {
        var carteira = new Carteira("user-1", "  Minha carteira  ", principal: true);

        TestAssert.Equal("Minha carteira", carteira.Nome, "Wallet name should be trimmed");
        TestAssert.Equal("MINHA CARTEIRA", carteira.NomeNormalizado, "Wallet name should have a stable uniqueness key");
        TestAssert.True(carteira.Principal, "The requested default wallet state should be kept");
        TestAssert.Equal("user-1", carteira.UsuarioId, "Wallet should keep its owner");
        await Task.CompletedTask;
    }

    public static async Task RejectsEmptyAndOverlongNames()
    {
        await TestAssert.ThrowsAsync<ArgumentException>(
            () => Task.Run(() => new Carteira("user-1", "   ")),
            "An empty wallet name must fail");
        await TestAssert.ThrowsAsync<ArgumentException>(
            () => Task.Run(() => new Carteira("user-1", new string('a', 81))),
            "A wallet name above 80 characters must fail");
        await TestAssert.ThrowsAsync<ArgumentException>(
            () => Task.Run(() => new Carteira("  ", "Minha carteira")),
            "An empty owner must fail");
    }

    public static async Task RenameUpdatesNormalizedName()
    {
        var carteira = new Carteira("user-1", "Minha carteira", principal: true);
        var createdAt = carteira.CriadaEm;

        carteira.Renomear("  Carteira da esposa ");

        TestAssert.Equal("Carteira da esposa", carteira.Nome, "Rename should trim the new name");
        TestAssert.Equal("CARTEIRA DA ESPOSA", carteira.NomeNormalizado, "Rename should update the uniqueness key");
        TestAssert.True(carteira.AtualizadaEm >= createdAt, "Rename should update the modification timestamp");
        await Task.CompletedTask;
    }
}
