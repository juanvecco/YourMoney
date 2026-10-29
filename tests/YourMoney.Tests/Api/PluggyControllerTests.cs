using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Api.Services;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class PluggyControllerTests
{
    public static async Task ConnectTokenUsesAuthenticatedUser()
    {
        var pluggy = new FakePluggyService();
        var controller = new PluggyController(
            pluggy,
            new FakeCurrentUserService("user-123"));

        var result = await controller.CriarConnectToken(
            new PluggyConnectTokenRequestDTO(),
            CancellationToken.None);

        var ok = result as OkObjectResult;
        TestAssert.True(ok != null, "Connect token should return 200 OK");
        var response = ok!.Value as PluggyConnectTokenResponseDTO;
        TestAssert.True(response != null, "Connect token should return typed response");
        TestAssert.Equal("connect-token", response!.AccessToken, "Connect token should be returned");
        TestAssert.Equal("user-123", pluggy.ClientUserId, "Authenticated user should scope Pluggy token");
    }

    public static async Task MissingConfigurationReturnsServiceUnavailable()
    {
        var controller = new PluggyController(
            new FakePluggyService { Failure = new PluggyConfigurationException() },
            new FakeCurrentUserService("user-123"));

        var result = await controller.CriarConnectToken(
            new PluggyConnectTokenRequestDTO(),
            CancellationToken.None);

        var unavailable = result as ObjectResult;
        TestAssert.True(unavailable != null, "Missing Pluggy settings should return an object result");
        TestAssert.Equal(503, unavailable!.StatusCode, "Missing Pluggy settings should return 503");
    }

    public static Task EndpointRequiresAuthorization()
    {
        var hasAuthorize = typeof(PluggyController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Any();
        TestAssert.True(hasAuthorize, "Pluggy controller should require authorization");
        return Task.CompletedTask;
    }

    public static async Task OverviewReturnsAllTypedExpenseCandidates()
    {
        var pluggy = new FakePluggyService
        {
            Overview = new PluggyOverviewDTO
            {
                Transactions =
                [
                    new PluggyTransactionPreviewDTO
                    {
                        TransactionId = "source-1",
                        Direction = "DEBIT",
                        PossibleDuplicateCandidates =
                        [
                            new PluggyDuplicateCandidateDTO { RecordType = "expense", RecordId = Guid.NewGuid() },
                            new PluggyDuplicateCandidateDTO { RecordType = "expense", RecordId = Guid.NewGuid() }
                        ]
                    }
                ]
            }
        };
        var controller = new PluggyController(pluggy, new FakeCurrentUserService("user-123"));

        var result = await controller.ObterVisaoGeralTransacoes(
            null,
            null,
            null,
            null,
            CancellationToken.None);

        var ok = result as OkObjectResult;
        TestAssert.True(ok?.Value is PluggyOverviewDTO, "Overview should return the typed response");
        var overview = (PluggyOverviewDTO)ok!.Value!;
        TestAssert.Equal(
            2,
            overview.Transactions[0].PossibleDuplicateCandidates.Count,
            "Every manual expense match should remain available for explicit selection");
    }

    public static async Task ExpenseReconciliationUsesAuthenticatedUserAndReturnsTypedResult()
    {
        var reconciliation = new PluggyExpenseReconciliationResultDTO
        {
            TransactionId = "source-1",
            Action = "link",
            RecordType = "expense",
            RecordId = Guid.NewGuid(),
            RegisteredAt = DateTimeOffset.UtcNow
        };
        var pluggy = new FakePluggyService { ReconciliationResult = reconciliation };
        var controller = new PluggyController(pluggy, new FakeCurrentUserService("user-123"));

        var result = await controller.ReconciliarDespesa(
            new PluggyExpenseReconciliationRequestDTO { TransactionId = "source-1", Action = "link" },
            CancellationToken.None);

        var ok = result as OkObjectResult;
        TestAssert.True(ok?.Value is PluggyExpenseReconciliationResultDTO,
            "Reconciliation should return the typed result");
        TestAssert.Equal("user-123", pluggy.ReconciliationUserId,
            "Reconciliation should use the authenticated owner");
        TestAssert.Equal("link", ((PluggyExpenseReconciliationResultDTO)ok!.Value!).Action,
            "Reconciliation should preserve the explicit action");
    }

    private sealed class FakePluggyService : IPluggyService
    {
        public string ClientUserId { get; private set; } = string.Empty;
        public string ReconciliationUserId { get; private set; } = string.Empty;
        public Exception? Failure { get; set; }
        public Exception? ReconciliationFailure { get; set; }
        public PluggyOverviewDTO Overview { get; set; } = new();
        public PluggyExpenseReconciliationResultDTO ReconciliationResult { get; set; } = new();

        public Task<PluggyCredentialStatusDTO> ObterConfiguracaoCredenciaisAsync(
            string usuarioId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PluggyCredentialStatusDTO());

        public Task<PluggyCredentialStatusDTO> SalvarConfiguracaoCredenciaisAsync(
            string usuarioId,
            PluggyCredentialRequestDTO request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PluggyCredentialStatusDTO { Configured = true, Source = "database", CanSave = true });

        public Task<PluggyConnectionDTO> RegistrarConexaoAsync(
            string usuarioId,
            string itemId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<PluggyConnectionDTO>> ListarConexoesAsync(
            string usuarioId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PluggyConnectionDTO>>(Array.Empty<PluggyConnectionDTO>());

        public Task<IReadOnlyList<PluggyAccountDTO>> ListarContasAsync(
            string usuarioId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PluggyAccountDTO>>(Array.Empty<PluggyAccountDTO>());

        public Task<IReadOnlyList<PluggyTransactionPreviewDTO>> ListarTransacoesPreviewAsync(
            string usuarioId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PluggyTransactionPreviewDTO>>(Array.Empty<PluggyTransactionPreviewDTO>());

        public Task<PluggyOverviewDTO> ObterVisaoGeralAsync(
            string usuarioId,
            PluggyOverviewFilterDTO filtro,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Overview);

        public Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(
            string usuarioId,
            PluggyTransactionImportRequestDTO request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PluggyTransactionImportResultDTO());

        public Task<PluggyExpenseReconciliationResultDTO> ReconciliarDespesaAsync(
            string usuarioId,
            PluggyExpenseReconciliationRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            ReconciliationUserId = usuarioId;
            if (ReconciliationFailure != null) throw ReconciliationFailure;
            return Task.FromResult(ReconciliationResult);
        }

        public Task RemoverConexaoAsync(
            string usuarioId,
            Guid connectionId,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<PluggyConnectTokenResponseDTO> CriarConnectTokenAsync(
            string clientUserId,
            Guid? itemId,
            CancellationToken cancellationToken = default)
        {
            ClientUserId = clientUserId;
            if (Failure != null) throw Failure;
            return Task.FromResult(new PluggyConnectTokenResponseDTO
            {
                AccessToken = "connect-token",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
            });
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public FakeCurrentUserService(string userId) => UserId = userId;
        public string UserId { get; }
        public bool IsAuthenticated => true;
    }
}

