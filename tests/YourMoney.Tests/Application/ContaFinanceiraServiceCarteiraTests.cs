using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Application.Services;
using YourMoney.Domain.Entities;
using YourMoney.Domain.Repositories;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class ContaFinanceiraServiceCarteiraTests
{
    public static async Task CreatesManualAccountWithinSelectedWallet()
    {
        var owner = new FakeCurrentUserService { UserId = "account-owner" };
        var wallets = new FakeWalletRepository(owner.UserId);
        var accounts = new FakeAccountRepository();
        var service = new ContaFinanceiraService(accounts, owner, wallets);

        var created = await service.CriarAsync(new ContaFinanceiraRequest
        {
            Descricao = "Banco Inter",
            CarteiraId = wallets.SecondWalletId
        });

        TestAssert.Equal(wallets.SecondWalletId, created.CarteiraId, "New manual account should be assigned to the requested owner wallet");
        TestAssert.Equal("account-owner", accounts.Items.Single().UsuarioId, "Account owner should come from the session");
    }

    public static async Task MovingAccountRequiresConfirmationAndPreservesRecordIdentity()
    {
        var owner = new FakeCurrentUserService { UserId = "account-owner" };
        var wallets = new FakeWalletRepository(owner.UserId);
        var accounts = new FakeAccountRepository();
        var existing = new ContaFinanceira("Conta ligada", owner.UserId, wallets.PrincipalWalletId);
        accounts.Items.Add(existing);
        var service = new ContaFinanceiraService(accounts, owner, wallets);

        await TestAssert.ThrowsAsync<ContaFinanceiraMoverConfirmacaoException>(() => service.MoverAsync(existing.Id, new MoverContaFinanceiraRequest
        {
            CarteiraId = wallets.SecondWalletId,
            ConfirmarTransferencia = false
        }), "Move without explicit confirmation should be rejected");
        var moved = await service.MoverAsync(existing.Id, new MoverContaFinanceiraRequest
        {
            CarteiraId = wallets.SecondWalletId,
            ConfirmarTransferencia = true
        });

        TestAssert.Equal(existing.Id, moved.Id, "Moving a bank account should preserve its identifier and linked records");
        TestAssert.Equal(wallets.SecondWalletId, moved.CarteiraId, "The account should move only after confirmation");
    }

    public static async Task ControllerRequiresAuthorizationAndReturnsConflictWithoutMoveConfirmation()
    {
        var authorized = typeof(ContaFinanceiraController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
        TestAssert.True(authorized, "Financial account routes must stay authenticated");
        var controller = new ContaFinanceiraController(new FakeAccountService());
        var result = await controller.Mover(Guid.NewGuid(), new MoverContaFinanceiraRequest
        {
            CarteiraId = Guid.NewGuid(),
            ConfirmarTransferencia = false
        });
        TestAssert.True(result is ObjectResult { StatusCode: 409 }, "An unconfirmed account move should return HTTP 409");
    }

    private sealed class FakeWalletRepository(string ownerId) : ICarteiraRepository
    {
        private readonly Carteira _principal = new(ownerId, "Minha carteira", principal: true);
        private readonly Carteira _second = new(ownerId, "Carteira da família");
        public Guid PrincipalWalletId => _principal.Id;
        public Guid SecondWalletId => _second.Id;

        public Task<List<Carteira>> ListarAsync(string usuarioId) => Task.FromResult(new List<Carteira> { _principal, _second });
        public Task<Carteira?> ObterAsync(Guid id, string usuarioId) => Task.FromResult(
            usuarioId == ownerId ? new[] { _principal, _second }.SingleOrDefault(wallet => wallet.Id == id) : null);
        public Task<Carteira?> ObterPrincipalAsync(string usuarioId) => Task.FromResult(usuarioId == ownerId ? _principal : null);
        public Task<Carteira> ObterOuCriarPrincipalAsync(string usuarioId) => Task.FromResult(_principal);
        public Task<bool> NomeExisteAsync(string usuarioId, string nomeNormalizado, Guid? excetoId = null) => Task.FromResult(false);
        public Task<bool> PossuiDependenciasAsync(Guid carteiraId, string usuarioId) => Task.FromResult(false);
        public Task AdicionarAsync(Carteira carteira) => Task.CompletedTask;
        public Task AtualizarAsync(Carteira carteira) => Task.CompletedTask;
        public Task RemoverAsync(Carteira carteira) => Task.CompletedTask;
    }

    private sealed class FakeAccountRepository : IContaFinanceiraRepository
    {
        public List<ContaFinanceira> Items { get; } = new();
        public Task<ContaFinanceira> GetByIdAsync(Guid id) => Task.FromResult(Items.Single(item => item.Id == id));
        public Task<ContaFinanceira> GetByIdAsync(Guid id, string usuarioId) => Task.FromResult(Items.Single(item => item.Id == id && item.UsuarioId == usuarioId));
        public Task AdicionarAsync(ContaFinanceira contaFinanceira) { Items.Add(contaFinanceira); return Task.CompletedTask; }
        public Task AtualizarAsync(ContaFinanceira contaFinanceira) => Task.CompletedTask;
        public Task RemoverAsync(Guid id) => Task.CompletedTask;
        public Task RemoverAsync(Guid id, string usuarioId) => Task.CompletedTask;
        public Task<List<ContaFinanceira>> ListarAsync() => Task.FromResult(Items.ToList());
        public Task<List<ContaFinanceira>> ListarAsync(string usuarioId) => Task.FromResult(Items.Where(item => item.UsuarioId == usuarioId).ToList());
        public Task<List<ContaFinanceira>> ListarAsync(string usuarioId, Guid carteiraId) => Task.FromResult(Items.Where(item => item.UsuarioId == usuarioId && item.CarteiraId == carteiraId).ToList());
        public Task<bool> ExisteAsync(Guid id) => Task.FromResult(Items.Any(item => item.Id == id));
        public Task<bool> ExisteAsync(Guid id, string usuarioId) => Task.FromResult(Items.Any(item => item.Id == id && item.UsuarioId == usuarioId));
        public Task<bool> TemVinculosAsync(Guid id, string usuarioId) => Task.FromResult(false);
    }

    private sealed class FakeAccountService : IContaFinanceiraService
    {
        public Task<ContaFinanceiraResponse> CriarAsync(ContaFinanceiraRequest request) => throw new NotSupportedException();
        public Task RemoverContaFinanceiraAsync(Guid id) => throw new NotSupportedException();
        public Task<ContaFinanceiraResponse> AtualizarAsync(Guid id, ContaFinanceiraRequest request) => throw new NotSupportedException();
        public Task<List<ContaFinanceiraResponse>> ListarAsync() => Task.FromResult(new List<ContaFinanceiraResponse>());
        public Task<List<ContaFinanceiraResponse>> ListarAsync(Guid? carteiraId) => Task.FromResult(new List<ContaFinanceiraResponse>());
        public Task<ContaFinanceiraResponse> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<ContaFinanceiraResponse> MoverAsync(Guid id, MoverContaFinanceiraRequest request) =>
            throw new ContaFinanceiraMoverConfirmacaoException();
    }
}
