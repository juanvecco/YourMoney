using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure;

public static class ContaFinanceiraRepositoryTests
{
    public static Task LinksAndOrderingStayOwnerScoped()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Infrastructure", "Repositories", "ContaFinanceiraRepository.cs"));
        TestAssert.True(source.Contains("OrderBy(c => c.Descricao)"), "Account list should use stable description ordering");
        TestAssert.True(source.Contains("Receitas.AnyAsync"), "Revenue links must block deletion");
        TestAssert.True(source.Contains("Despesas.AnyAsync"), "Expense links must block deletion");
        TestAssert.True(source.Contains("UsuarioId == usuarioId"), "Link checks must stay owner scoped");
        return Task.CompletedTask;
    }
}
