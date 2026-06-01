using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class FinancialRepositoryIsolationTests
    {
        public static Task RepositoriesFilterReadsByUsuarioId()
        {
            foreach (var file in RepositoryFiles())
            {
                var source = File.ReadAllText(file);
                var name = Path.GetFileName(file);

                TestAssert.True(source.Contains("UsuarioId == usuarioId"), $"{name} should filter user-scoped query paths by usuarioId");
                TestAssert.True(source.Contains("GetByIdAsync(Guid id, string usuarioId)") || source.Contains("GetByIdAsync(Guid id, string usuarioId)"), $"{name} should expose a user-scoped detail lookup");
            }

            return Task.CompletedTask;
        }

        public static Task PeriodAndListQueriesIncludeUsuarioId()
        {
            var files = RepositoryFiles();
            var periodRepositories = files.Where(f =>
                Path.GetFileName(f) is "DespesaRepository.cs" or "ReceitaRepository.cs" or "InvestimentoRepository.cs");

            foreach (var file in periodRepositories)
            {
                var source = File.ReadAllText(file);
                var name = Path.GetFileName(file);

                TestAssert.True(source.Contains("Data") && source.Contains("usuarioId"), $"{name} should include usuarioId in date/period queries");
                TestAssert.True(source.Contains("ListarAsync(string usuarioId)") || source.Contains("GetAllAsync(string usuarioId)"), $"{name} should expose user-scoped list methods");
            }

            return Task.CompletedTask;
        }

        private static IEnumerable<string> RepositoryFiles()
        {
            var repositoryRoot = RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Repositories");
            return new[]
            {
                "CategoriaRepository.cs",
                "ContaFinanceiraRepository.cs",
                "DespesaRepository.cs",
                "InvestimentoRepository.cs",
                "MetaRepository.cs",
                "ReceitaRepository.cs"
            }.Select(name => Path.Combine(repositoryRoot, name));
        }
    }
}
