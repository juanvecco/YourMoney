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
    public static async Task ConnectionStatusUsesAuthenticatedOwner()
    {
        var pluggy = new FakePluggyService { HasConnections = true };
        var controller = new PluggyController(pluggy, new FakeCurrentUserService("owner-123"));

        var result = await controller.ObterStatusConexao(CancellationToken.None);

        TestAssert.True(result is OkObjectResult, "Connection status should return 200 OK");
        TestAssert.Equal("owner-123", pluggy.ConnectionStatusUserId,
            "Connection status must use the authenticated user");
        TestAssert.True(System.Text.Json.JsonSerializer.Serialize(((OkObjectResult)result).Value)
            .Contains("\"hasConnections\":true", StringComparison.Ordinal),
            "Connection status should expose only the boolean result");
    }

    public static async Task ConnectTokenUsesAuthenticatedUser()
    {
        var pluggy = new FakePluggyService();
        var controller = new PluggyController(
            pluggy,
            new FakeCurrentUserService("user-123"));

        var result = await controller.CriarConnectToken(
            new PluggyConnectTokenRequestDTO(),
            null,
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
            null,
            CancellationToken.None);

        var unavailable = result as ObjectResult;
        TestAssert.True(unavailable != null, "Missing Pluggy settings should return an object result");
        TestAssert.Equal(503, unavailable!.StatusCode, "Missing Pluggy settings should return 503");
    }

    public static async Task CredentialAndConnectionEndpointsForwardWalletAndMapLimitConflict()
    {
        var walletId = Guid.NewGuid();
        var pluggy = new FakePluggyService();
        var controller = new PluggyController(pluggy, new FakeCurrentUserService("user-123"));

        var status = await controller.ObterConfiguracaoCredenciais(walletId, CancellationToken.None);
        TestAssert.True(status is OkObjectResult, "Credential status should be returned for the selected wallet");
        TestAssert.Equal(walletId, pluggy.RequestedWalletId, "Credential status must use the selected wallet");

        var saved = await controller.SalvarConfiguracaoCredenciais(
            new PluggyCredentialRequestDTO { ClientId = "client", ClientSecret = "secret" },
            walletId,
            CancellationToken.None);
        TestAssert.True(saved is OkObjectResult, "Wallet credentials should return the safe status contract");
        TestAssert.Equal(walletId, pluggy.RequestedWalletId, "Credential save must use the selected wallet");

        pluggy.Failure = new PluggyConnectionLimitException();
        var connect = await controller.CriarConnectToken(new PluggyConnectTokenRequestDTO(), walletId, CancellationToken.None);
        TestAssert.True(connect is ConflictObjectResult, "The per-wallet connection limit should map to HTTP 409");
        TestAssert.Equal(walletId, pluggy.RequestedWalletId, "Connect token must use the selected wallet");
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
            null,
            CancellationToken.None);

        var ok = result as OkObjectResult;
        TestAssert.True(ok?.Value is PluggyExpenseReconciliationResultDTO,
            "Reconciliation should return the typed result");
        TestAssert.Equal("user-123", pluggy.ReconciliationUserId,
            "Reconciliation should use the authenticated owner");
        TestAssert.Equal("link", ((PluggyExpenseReconciliationResultDTO)ok!.Value!).Action,
            "Reconciliation should preserve the explicit action");
    }

    public static async Task InvestmentReconciliationUsesAuthenticatedUserAndReturnsTypedResult()
    {
        var reconciliation = new PluggyInvestmentReconciliationResultDTO
        {
            TransactionId = "source-investment-1",
            Action = "link",
            RecordType = "investment",
            RecordId = Guid.NewGuid(),
            RegisteredAt = DateTimeOffset.UtcNow
        };
        var pluggy = new FakePluggyService { InvestmentReconciliationResult = reconciliation };
        var controller = new PluggyController(pluggy, new FakeCurrentUserService("user-123"));

        var result = await controller.ReconciliarInvestimento(
            new PluggyInvestmentReconciliationRequestDTO { TransactionId = "source-investment-1", Action = "link" },
            null,
            CancellationToken.None);

        var ok = result as OkObjectResult;
        TestAssert.True(ok?.Value is PluggyInvestmentReconciliationResultDTO,
            "Investment reconciliation should return its typed response");
        TestAssert.Equal("user-123", pluggy.ReconciliationUserId,
            "Investment reconciliation should use the authenticated owner");
        TestAssert.Equal("investment", ((PluggyInvestmentReconciliationResultDTO)ok!.Value!).RecordType,
            "The result should identify the investment record type");
    }

    public static async Task InvestmentReconciliationMapsReviewConflict()
    {
        var pluggy = new FakePluggyService
        {
            ReconciliationFailure = new PluggyInvestmentReconciliationConflictException(
                "candidate_changed", "Atualize a lista e revise novamente.")
        };
        var controller = new PluggyController(pluggy, new FakeCurrentUserService("user-123"));

        var result = await controller.ReconciliarInvestimento(
            new PluggyInvestmentReconciliationRequestDTO { TransactionId = "source-investment-1", Action = "replace" },
            null,
            CancellationToken.None);

        var conflict = result as ConflictObjectResult;
        TestAssert.True(conflict != null, "A stale investment candidate should return HTTP 409");
        TestAssert.Equal("user-123", pluggy.ReconciliationUserId,
            "Conflict handling should still use the authenticated owner");
    }

    private sealed class FakePluggyService : IPluggyService
    {
        public bool HasConnections { get; set; }
        public string ConnectionStatusUserId { get; private set; } = string.Empty;
        public string ClientUserId { get; private set; } = string.Empty;
        public string ReconciliationUserId { get; private set; } = string.Empty;
        public Guid? RequestedWalletId { get; private set; }
        public Exception? Failure { get; set; }
        public Exception? ReconciliationFailure { get; set; }
        public PluggyOverviewDTO Overview { get; set; } = new();
        public PluggyExpenseReconciliationResultDTO ReconciliationResult { get; set; } = new();
        public PluggyInvestmentReconciliationResultDTO InvestmentReconciliationResult { get; set; } = new();

        public Task<PluggyCredentialStatusDTO> ObterConfiguracaoCredenciaisAsync(
            string usuarioId,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null)
        {
            RequestedWalletId = carteiraId;
            return Task.FromResult(new PluggyCredentialStatusDTO());
        }

        public Task<PluggyCredentialStatusDTO> SalvarConfiguracaoCredenciaisAsync(
            string usuarioId,
            PluggyCredentialRequestDTO request,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null)
        {
            RequestedWalletId = carteiraId;
            return Task.FromResult(new PluggyCredentialStatusDTO { Configured = true, Source = "database", CanSave = true });
        }

        public Task<PluggyConnectionDTO> RegistrarConexaoAsync(
            string usuarioId,
            string itemId,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<PluggyConnectionDTO>> ListarConexoesAsync(
            string usuarioId,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null) =>
            Task.FromResult<IReadOnlyList<PluggyConnectionDTO>>(Array.Empty<PluggyConnectionDTO>());

        public Task<bool> PossuiConexoesAsync(string usuarioId, CancellationToken cancellationToken = default)
        {
            ConnectionStatusUserId = usuarioId;
            return Task.FromResult(HasConnections);
        }

        public Task<IReadOnlyList<PluggyAccountDTO>> ListarContasAsync(
            string usuarioId,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null) =>
            Task.FromResult<IReadOnlyList<PluggyAccountDTO>>(Array.Empty<PluggyAccountDTO>());

        public Task<PluggyInvestmentAssetsOverviewDTO> ObterAtivosInvestimentoAsync(
            string usuarioId, Guid? connectionId = null, bool retryFailed = false,
            CancellationToken cancellationToken = default, Guid? carteiraId = null) =>
            Task.FromResult(new PluggyInvestmentAssetsOverviewDTO());

        public Task<PluggyInvestmentAssetReconciliationResultDTO> ReconciliarAtivoInvestimentoAsync(
            string usuarioId, PluggyInvestmentAssetReconciliationRequestDTO request,
            CancellationToken cancellationToken = default, Guid? carteiraId = null)
        {
            ReconciliationUserId = usuarioId;
            if (ReconciliationFailure != null) throw ReconciliationFailure;
            return Task.FromResult(new PluggyInvestmentAssetReconciliationResultDTO());
        }

        public Task<IReadOnlyList<PluggyTransactionPreviewDTO>> ListarTransacoesPreviewAsync(
            string usuarioId,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null) =>
            Task.FromResult<IReadOnlyList<PluggyTransactionPreviewDTO>>(Array.Empty<PluggyTransactionPreviewDTO>());

        public Task<PluggyOverviewDTO> ObterVisaoGeralAsync(
            string usuarioId,
            PluggyOverviewFilterDTO filtro,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null) =>
            Task.FromResult(Overview);

        public Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(
            string usuarioId,
            PluggyTransactionImportRequestDTO request,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null) =>
            Task.FromResult(new PluggyTransactionImportResultDTO());

        public Task<PluggyExpenseReconciliationResultDTO> ReconciliarDespesaAsync(
            string usuarioId,
            PluggyExpenseReconciliationRequestDTO request,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null)
        {
            ReconciliationUserId = usuarioId;
            if (ReconciliationFailure != null) throw ReconciliationFailure;
            return Task.FromResult(ReconciliationResult);
        }

        public Task<PluggyInvestmentReconciliationResultDTO> ReconciliarInvestimentoAsync(
            string usuarioId,
            PluggyInvestmentReconciliationRequestDTO request,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null)
        {
            ReconciliationUserId = usuarioId;
            if (ReconciliationFailure != null) throw ReconciliationFailure;
            return Task.FromResult(InvestmentReconciliationResult);
        }

        public Task RemoverConexaoAsync(
            string usuarioId,
            Guid connectionId,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null) => Task.CompletedTask;

        public Task<PluggyConnectTokenResponseDTO> CriarConnectTokenAsync(
            string clientUserId,
            Guid? itemId,
            CancellationToken cancellationToken = default,
            Guid? carteiraId = null)
        {
            ClientUserId = clientUserId;
            RequestedWalletId = carteiraId;
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

