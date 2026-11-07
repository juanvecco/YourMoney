using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class CarteiraServiceTests
{
    public static async Task ListCreatesDefaultWalletForCurrentOwner()
    {
        var repository = new FakeCarteiraRepository();
        var service = new CarteiraService(repository, new FakeCurrentUserService("user-a"));

        var wallets = await service.ListarAsync();

        TestAssert.Equal(1, wallets.Count, "Listing without a wallet should create the default");
        TestAssert.Equal("user-a", repository.Wallets.Single().UsuarioId, "Default should belong to authenticated user");
        TestAssert.True(wallets[0].Principal, "The initial wallet should be principal");
        TestAssert.Equal("Minha carteira", wallets[0].Nome, "The initial name should be clear");
    }

    public static async Task CreateUsesCurrentOwnerAndRejectsDuplicateNormalizedName()
    {
        var repository = new FakeCarteiraRepository();
        var service = new CarteiraService(repository, new FakeCurrentUserService("user-a"));
        var created = await service.CriarAsync(new CarteiraRequestDTO { Nome = "Carteira da esposa" });

        TestAssert.Equal("user-a", repository.Wallets.Single().UsuarioId, "The owner must come from the session");
        TestAssert.True(!created.Principal, "Additional wallets must not replace the default");

        await TestAssert.ThrowsAsync<CarteiraNomeDuplicadoException>(
            () => service.CriarAsync(new CarteiraRequestDTO { Nome = " CARTEIRA DA ESPOSA " }),
            "Duplicate names should be rejected after normalization");

        var foreignWallet = new Carteira("user-b", "Carteira particular");
        repository.Wallets.Add(foreignWallet);
        await TestAssert.ThrowsAsync<CarteiraNaoEncontradaException>(
            () => service.RenomearAsync(foreignWallet.Id, new CarteiraRequestDTO { Nome = "Sem acesso" }),
            "A wallet owned by another user must appear not found");
    }

    public static async Task RenameAndDeleteRequireWalletOwnershipAndEmptyAdditionalWallet()
    {
        var repository = new FakeCarteiraRepository();
        var defaultWallet = await repository.ObterOuCriarPrincipalAsync("user-a");
        var additional = new Carteira("user-a", "Outra carteira");
        repository.Wallets.Add(additional);
        var service = new CarteiraService(repository, new FakeCurrentUserService("user-a"));

        var renamed = await service.RenomearAsync(additional.Id, new CarteiraRequestDTO { Nome = "Nova carteira" });
        TestAssert.Equal("Nova carteira", renamed.Nome, "Rename should return the changed name");

        await TestAssert.ThrowsAsync<CarteiraPrincipalException>(
            () => service.RemoverAsync(defaultWallet.Id),
            "The principal wallet must not be deleted");

        repository.HasDependencies = true;
        await TestAssert.ThrowsAsync<CarteiraComDependenciasException>(
            () => service.RemoverAsync(additional.Id),
            "A wallet with accounts or connections must not be deleted");

        repository.HasDependencies = false;
        await service.RemoverAsync(additional.Id);
        TestAssert.True(repository.Wallets.All(wallet => wallet.Id != additional.Id), "An empty additional wallet should be removable");
    }

    private sealed class FakeCurrentUserService(string userId) : ICurrentUserService
    {
        public string UserId { get; } = userId;
        public bool IsAuthenticated => true;
    }

    private sealed class FakeCarteiraRepository : ICarteiraRepository
    {
        public List<Carteira> Wallets { get; } = [];
        public bool HasDependencies { get; set; }

        public Task<List<Carteira>> ListarAsync(string usuarioId) => Task.FromResult(
            Wallets.Where(wallet => wallet.UsuarioId == usuarioId).ToList());

        public Task<Carteira?> ObterAsync(Guid id, string usuarioId) => Task.FromResult(
            Wallets.SingleOrDefault(wallet => wallet.Id == id && wallet.UsuarioId == usuarioId));

        public Task<Carteira?> ObterPrincipalAsync(string usuarioId) => Task.FromResult(
            Wallets.SingleOrDefault(wallet => wallet.UsuarioId == usuarioId && wallet.Principal));

        public async Task<Carteira> ObterOuCriarPrincipalAsync(string usuarioId)
        {
            var existing = await ObterPrincipalAsync(usuarioId);
            if (existing is not null)
                return existing;
            var created = new Carteira(usuarioId, "Minha carteira", principal: true);
            Wallets.Add(created);
            return created;
        }

        public Task<bool> NomeExisteAsync(string usuarioId, string nomeNormalizado, Guid? excetoId = null) => Task.FromResult(
            Wallets.Any(wallet => wallet.UsuarioId == usuarioId
                && wallet.NomeNormalizado == nomeNormalizado
                && (!excetoId.HasValue || wallet.Id != excetoId.Value)));

        public Task<bool> PossuiDependenciasAsync(Guid carteiraId, string usuarioId) => Task.FromResult(HasDependencies);

        public Task AdicionarAsync(Carteira carteira)
        {
            Wallets.Add(carteira);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Carteira carteira) => Task.CompletedTask;

        public Task RemoverAsync(Carteira carteira)
        {
            Wallets.Remove(carteira);
            return Task.CompletedTask;
        }
    }
}
