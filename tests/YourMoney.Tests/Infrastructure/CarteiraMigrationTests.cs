using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure;

public static class CarteiraMigrationTests
{
    public static Task BackfillsPrincipalWalletsWithoutChangingLegacyIdentifiers()
    {
        var migrations = RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Migrations");
        var source = File.ReadAllText(Directory.GetFiles(migrations, "*_CarteirasPluggy.cs").Single());
        TestAssert.True(source.Contains("INSERT INTO [tbCarteira]", StringComparison.Ordinal), "Migration should create a principal wallet for legacy owners");
        TestAssert.True(source.Contains("UNION", StringComparison.Ordinal), "Migration should collect owners from every legacy wallet resource");
        TestAssert.True(source.Contains("UPDATE account SET [CarteiraId]", StringComparison.Ordinal), "Existing financial account IDs should be associated in place");
        TestAssert.True(source.Contains("UPDATE credential SET [CarteiraId]", StringComparison.Ordinal), "Stored encrypted credentials should remain in place");
        TestAssert.True(source.Contains("UPDATE connection SET [CarteiraId]", StringComparison.Ordinal), "Existing Pluggy item IDs should remain in place");
        TestAssert.True(!source.Contains("DELETE FROM [tbContaFinanceira]", StringComparison.OrdinalIgnoreCase), "Migration must not delete existing financial accounts");
        TestAssert.True(!source.Contains("DELETE FROM [tbPluggyConnection]", StringComparison.OrdinalIgnoreCase), "Migration must not delete existing provider connections");
        TestAssert.True(source.Contains("THROW 51000", StringComparison.Ordinal), "Rollback must stop while secondary wallets still exist");
        return Task.CompletedTask;
    }
}
