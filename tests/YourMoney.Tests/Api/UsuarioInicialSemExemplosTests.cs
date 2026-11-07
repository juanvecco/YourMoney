using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using YourMoney.Api.Services;
using YourMoney.Application.Services;
using YourMoney.Application.Interfaces;
using YourMoney.Domain.Entities;
using YourMoney.Infrastructure.Persistence;
using YourMoney.Infrastructure.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class UsuarioInicialSemExemplosTests
{
    public static async Task CreatesEmptyFinancesAndDisabledAdvancedRules()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"new-user-{Guid.NewGuid():N}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using var db = new AppDbContext(options);
        var service = new UsuarioDemonstracaoService(db);

        await service.CriarDadosIniciaisAsync("new-user");
        TestAssert.Equal(1, await db.Carteiras.CountAsync(), "New user must have a wallet");
        TestAssert.True(await db.Categorias.AnyAsync(), "New user must have category choices");
        TestAssert.Equal(0, await db.ContaFinanceira.CountAsync(), "No example bank account may appear");
        TestAssert.Equal(0, await db.Receitas.CountAsync(), "No fictitious salary may appear");
        TestAssert.Equal(0, await db.Despesas.CountAsync(), "No fictitious expense may appear");
        TestAssert.Equal(RegraNegocioCatalogo.Todas.Count, await db.RegrasNegocioUsuario.CountAsync(),
            "Every advanced rule needs a new-account choice");
        TestAssert.True(await db.RegrasNegocioUsuario.AllAsync(rule => !rule.Ativa),
            "Advanced functions start off for new accounts");

        await service.CriarDadosIniciaisAsync("new-user");
        TestAssert.Equal(RegraNegocioCatalogo.Todas.Count, await db.RegrasNegocioUsuario.CountAsync(),
            "Preparing the account twice must not duplicate rules");
    }

    public static async Task RuleChangesPreserveExistingFinancialRows()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"preserve-history-{Guid.NewGuid():N}")
            .Options;
        await using var db = new AppDbContext(options);
        var date = new DateTime(2026, 11, 7);
        var revenue = new Receita("Salário real", 3200.25m, date, new DateTime(2026, 11, 1), "owner");
        db.Receitas.Add(revenue);
        await db.SaveChangesAsync();
        var service = new RegraNegocioService(new RegraNegocioUsuarioRepository(db), new FakeCurrentUser("owner"));

        await service.AtualizarAsync(RegraNegocioCatalogo.ReceitasRecorrentes, false);
        await service.AtualizarAsync(RegraNegocioCatalogo.ReceitasRecorrentes, true);

        var stored = await db.Receitas.SingleAsync();
        TestAssert.Equal(revenue.Id, stored.Id, "Toggling rules must preserve the revenue ID");
        TestAssert.Equal(3200.25m, stored.Valor, "Toggling rules must preserve exact money values");
    }

    private sealed class FakeCurrentUser(string id) : ICurrentUserService
    {
        public string UserId { get; } = id;
        public bool IsAuthenticated => true;
    }
}
