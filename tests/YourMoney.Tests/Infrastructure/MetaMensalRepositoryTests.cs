using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class MetaMensalRepositoryTests
    {
        public static Task RepositoryAndConfigurationStayOwnerScoped()
        {
            var repositoryPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Repositories", "MetaMensalRepository.cs");
            var repositorySource = File.ReadAllText(repositoryPath);

            TestAssert.True(repositorySource.Contains("meta.UsuarioId == usuarioId"), "Repository queries should filter by owner");
            TestAssert.True(repositorySource.Contains("meta.MesReferencia.Month == mes"), "Repository should filter by reference month");
            TestAssert.True(repositorySource.Contains("meta.MesReferencia.Year == ano"), "Repository should filter by reference year");

            var configurationPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Configurations", "MetaMensalConfiguration.cs");
            var configurationSource = File.ReadAllText(configurationPath);

            TestAssert.True(configurationSource.Contains("tbMetaMensal"), "Configuration should map the monthly goal table");
            TestAssert.True(configurationSource.Contains("decimal(9,4)"), "Percentage should keep decimal precision");
            TestAssert.True(configurationSource.Contains("IX_MetaMensal_Usuario_MesReferencia"), "Configuration should index owner and month");

            var dbContextPath = RepositoryTestPaths.InYourMoney(
                "src", "YourMoney.Infrastructure", "Persistence", "AppDbContext.cs");
            var dbContextSource = File.ReadAllText(dbContextPath);

            TestAssert.True(dbContextSource.Contains("GetColumnType() == null"), "Default decimal mapping should not override explicit precision");

            return Task.CompletedTask;
        }
    }
}
