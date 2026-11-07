using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class RegraNegocioServiceTests
{
    public static async Task DefaultsAllAccountsToDisabledAndSeparatesRecurrences()
    {
        var repo = new FakeRepository();
        var service = new RegraNegocioService(repo, new FakeCurrentUser("existing-user"));
        var initial = await service.ListarAsync();
        TestAssert.True(initial.All(rule => !rule.Ativa && !rule.Efetiva), "Missing choices must be off for existing users");

        await service.AtualizarAsync(RegraNegocioCatalogo.DespesasRecorrentes, true);
        TestAssert.True(await service.EstaAtivaAsync(RegraNegocioCatalogo.DespesasRecorrentes), "Expense recurrence can be enabled");
        TestAssert.True(!await service.EstaAtivaAsync(RegraNegocioCatalogo.ReceitasRecorrentes), "Revenue recurrence stays off");

        var other = new RegraNegocioService(repo, new FakeCurrentUser("other"));
        TestAssert.True(!await other.EstaAtivaAsync(RegraNegocioCatalogo.DespesasRecorrentes), "Other users stay off");
    }

    public static async Task DependencyDisablesEffectWithoutErasingPreference()
    {
        var service = new RegraNegocioService(new FakeRepository(), new FakeCurrentUser("owner"));
        await service.AtualizarAsync(RegraNegocioCatalogo.ReservaEmergencia, true);
        await service.AtualizarAsync(RegraNegocioCatalogo.ReceitasRecorrentes, true);
        await service.AtualizarAsync(RegraNegocioCatalogo.ReceitasRecorrentes, false);
        var reserve = (await service.ListarAsync()).Single(rule => rule.Codigo == RegraNegocioCatalogo.ReservaEmergencia);
        TestAssert.True(reserve.Ativa, "Child preference must remain selected");
        TestAssert.True(!reserve.Efetiva, "Child must be ineffective while parent is off");
        await service.AtualizarAsync(RegraNegocioCatalogo.ReceitasRecorrentes, true);
        TestAssert.True(await service.EstaAtivaAsync(RegraNegocioCatalogo.ReservaEmergencia), "Reactivation restores effect");
    }

    public static async Task RejectsUnknownRules()
    {
        var service = new RegraNegocioService(new FakeRepository(), new FakeCurrentUser("owner"));
        await TestAssert.ThrowsAsync<KeyNotFoundException>(
            () => service.AtualizarAsync("unknown", false), "Unknown keys must not be persisted");
    }

    private sealed class FakeCurrentUser(string id) : ICurrentUserService
    {
        public string UserId { get; } = id;
        public bool IsAuthenticated => true;
    }

    private sealed class FakeRepository : IRegraNegocioUsuarioRepository
    {
        private readonly List<RegraNegocioUsuario> _items = new();

        public Task<IReadOnlyList<RegraNegocioUsuario>> ListarAsync(string usuarioId) =>
            Task.FromResult<IReadOnlyList<RegraNegocioUsuario>>(
                _items.Where(item => item.UsuarioId == usuarioId).ToArray());

        public Task SalvarAsync(string usuarioId, string codigo, bool ativa)
        {
            var existing = _items.SingleOrDefault(item => item.UsuarioId == usuarioId && item.Codigo == codigo);
            if (existing is null) _items.Add(new RegraNegocioUsuario(usuarioId, codigo, ativa));
            else existing.Alterar(ativa);
            return Task.CompletedTask;
        }
    }
}
