using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class ReceitaRepositoryReferenceTests
    {
        public static Task RepositoryUsesReferenceWithLegacyFallback()
        {
            var repositoryPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "ReceitaRepository.cs");
            var source = File.ReadAllText(repositoryPath);

            TestAssert.True(source.Contains("r.MesReferencia.HasValue"), "Repository should prefer MesReferencia");
            TestAssert.True(source.Contains("!r.MesReferencia.HasValue && r.Data.Month"), "Repository should fall back to Data for legacy rows");
            TestAssert.True(source.Contains("r.UsuarioId == usuarioId"), "Reference queries should remain owner scoped");
            return Task.CompletedTask;
        }

        public static Task ConfigurationMapsExistingNullableDateColumn()
        {
            var configurationPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Configurations", "ReceitaConfiguration.cs");
            var source = File.ReadAllText(configurationPath);

            TestAssert.True(source.Contains("HasColumnName(\"mesReferencia\")"), "Configuration should map the existing column");
            TestAssert.True(source.Contains("HasColumnType(\"date\")"), "Reference column should use SQL date");
            TestAssert.True(source.Contains("IsRequired(false)"), "Existing legacy rows should allow null reference");
            return Task.CompletedTask;
        }
    }
}
