using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure;

public static class InvestimentoOrganizacaoPersistenceTests
{
    public static Task MapsNullableOwnedOrganizationSafely()
    {
        var investmentConfig = File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Configurations", "InvestimentoConfiguration.cs"));
        var groupConfig = File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Configurations", "GrupoObjetivoInvestimentoConfiguration.cs"));
        var migration = File.ReadAllText(Directory.GetFiles(RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Migrations"), "*_AddOrganizacaoInvestimentos.cs").Single());

        TestAssert.True(groupConfig.Contains("NomeNormalizado") && groupConfig.Contains("IsUnique()"), "Group normalized name should be unique per owner");
        TestAssert.True(groupConfig.Contains("IsRowVersion()") && groupConfig.Contains("IsRequired()"), "Group should use required rowversion");
        TestAssert.True(investmentConfig.Contains("HasPrincipalKey") && investmentConfig.Contains("UsuarioId"), "Investment/group FK should include ownership");
        TestAssert.True(investmentConfig.Contains("DeleteBehavior.Restrict"), "Group deletion should be restrictive");
        TestAssert.True(investmentConfig.Contains("CK_Investimento_TipoAtivo"), "Asset type should use a database check constraint");
        TestAssert.True(migration.Contains("GrupoObjetivoInvestimentoId") && migration.Contains("nullable: true"), "Organization columns should preserve historical nulls");
        TestAssert.True(!migration.Contains("UpdateData") && !migration.Contains("InsertData"), "Migration should not infer or seed organization");
        return Task.CompletedTask;
    }
}
