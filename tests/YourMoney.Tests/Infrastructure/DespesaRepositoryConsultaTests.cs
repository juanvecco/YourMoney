using Microsoft.EntityFrameworkCore;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Enums;
using YourMoney.Infrastructure.Persistence;
using YourMoney.Infrastructure.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class DespesaRepositoryConsultaTests
    {
        public static async Task PaginatesBeforeMaterializingExpensesAndKeepsNetTotals()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"despesas-paginadas-{Guid.NewGuid()}").Options;
            await using var context = new AppDbContext(options);
            var conta = Guid.NewGuid();
            var outraConta = Guid.NewGuid();
            var categoria = Guid.NewGuid();
            var primeira = new Despesa("Mercado", 200m, new DateTime(2026, 11, 2), conta, categoria, "user-a");
            var segunda = new Despesa("Aluguel", 900m, new DateTime(2026, 11, 4), conta, categoria, "user-a");
            var terceira = new Despesa("Lazer", 80m, new DateTime(2026, 11, 3), outraConta, categoria, "user-a");
            context.Despesas.AddRange(primeira, segunda, terceira,
                new Despesa("Outro usuário", 999m, new DateTime(2026, 11, 5), conta, categoria, "user-b"));
            context.Receitas.Add(new Receita("Reembolso", 50m, new DateTime(2026, 11, 6),
                new DateTime(2026, 11, 1), "user-a", NaturezaReceita.Reembolso, primeira.Id));
            await context.SaveChangesAsync();

            var repository = new DespesaRepository(context);
            var page = await repository.ConsultarPaginadoAsync(11, 2026, "user-a", null, null!, 1, 1);
            TestAssert.Equal(3, page.TotalResultados, "Summary includes all owner expenses");
            TestAssert.Equal(1, page.Itens.Count, "Database query returns only the requested page");
            TestAssert.Equal("Aluguel", page.Itens.Single().Descricao, "Page keeps newest-first ordering");
            TestAssert.Equal(1050m, page.TotaisPorConta.Single(total => total.IdContaFinanceira == conta).ValorLiquido,
                "Account total includes reimbursement outside the current page");
            TestAssert.Equal(80m, page.TotaisPorConta.Single(total => total.IdContaFinanceira == outraConta).ValorLiquido,
                "Summary includes every filtered account");

            var lastPage = await repository.ConsultarPaginadoAsync(11, 2026, "user-a", null, null!, 99, 1);
            TestAssert.Equal(3, lastPage.PaginaAtual, "Out-of-range page clamps to the last page");
            TestAssert.Equal("Mercado", lastPage.Itens.Single().Descricao, "Last page has the oldest owner expense");
        }

        public static async Task PaginatesAndAggregatesInPostgreSql()
        {
            var connectionString = Environment.GetEnvironmentVariable("YM_TEST_POSTGRES")
                ?? throw new InvalidOperationException("YM_TEST_POSTGRES must point to an isolated test database.");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString).Options;
            await using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var usuarioId = $"pg-{Guid.NewGuid():N}";
            var carteira = new Carteira(usuarioId, "Principal", true);
            var conta = new ContaFinanceira("Cartão", usuarioId, carteira.Id);
            var categoria = new Categoria("Mercado", TipoTransacao.Despesa, usuarioId);
            var primeira = new Despesa("Compra", 200m, new DateTime(2026, 11, 2), conta.Id, categoria.Id, usuarioId);
            var segunda = new Despesa("Outra compra", 100m, new DateTime(2026, 11, 3), conta.Id, categoria.Id, usuarioId);
            context.Carteiras.Add(carteira);
            context.ContaFinanceira.Add(conta);
            context.Categorias.Add(categoria);
            context.Despesas.AddRange(primeira, segunda);
            context.Receitas.Add(new Receita("Reembolso", 50m, new DateTime(2026, 11, 4),
                new DateTime(2026, 11, 1), usuarioId, NaturezaReceita.Reembolso, primeira.Id));
            await context.SaveChangesAsync();

            var page = await new DespesaRepository(context)
                .ConsultarPaginadoAsync(11, 2026, usuarioId, null, null!, 1, 1);
            TestAssert.Equal(2, page.TotalResultados, "PostgreSQL counts all filtered expenses");
            TestAssert.Equal(1, page.Itens.Count, "PostgreSQL returns only the requested page");
            TestAssert.Equal(250m, page.TotaisPorConta.Single().ValorLiquido,
                "PostgreSQL aggregates reimbursements across the full filtered set");
            await transaction.RollbackAsync();
        }

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
