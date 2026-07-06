using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class DespesaRepositoryConsultaTests
    {
        public static Task RepositoryConsultaIsUserScopedAndFilterable()
        {
            var source = File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Repositories", "DespesaRepository.cs"));

            TestAssert.True(source.Contains("ConsultarAsync"), "Repository should expose consulta method");
            TestAssert.True(source.Contains("d.UsuarioId == usuarioId"), "Consulta should filter by authenticated user");
            TestAssert.True(source.Contains("idContaFinanceira == null") && source.Contains("d.IdContaFinanceira"), "Consulta should filter by account");
            TestAssert.True(source.Contains("idsCategoria.Contains(d.IdCategoria)"), "Consulta should filter by resolved category ids");
            TestAssert.True(source.Contains("OrderByDescending(d => d.Data)"), "Consulta should preserve recent-first ordering");

            return Task.CompletedTask;
        }
    }
}
