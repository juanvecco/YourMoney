using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IPluggyService
{
    Task<PluggyCredentialStatusDTO> ObterConfiguracaoCredenciaisAsync(
        string usuarioId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyCredentialStatusDTO> SalvarConfiguracaoCredenciaisAsync(
        string usuarioId,
        PluggyCredentialRequestDTO request,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyConnectTokenResponseDTO> CriarConnectTokenAsync(
        string clientUserId,
        Guid? connectionId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyConnectionDTO> RegistrarConexaoAsync(
        string usuarioId,
        string itemId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyConnectionDTO> RegistrarConexaoAsync(
        string usuarioId,
        string itemId,
        Guid? connectionId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null) =>
        RegistrarConexaoAsync(usuarioId, itemId, cancellationToken, carteiraId);

    Task<IReadOnlyList<PluggyConnectionDTO>> ListarConexoesAsync(
        string usuarioId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<bool> PossuiConexoesAsync(string usuarioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PluggyAccountDTO>> ListarContasAsync(
        string usuarioId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyInvestmentAssetsOverviewDTO> ObterAtivosInvestimentoAsync(
        string usuarioId,
        Guid? connectionId = null,
        bool retryFailed = false,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyInvestmentAssetReconciliationResultDTO> ReconciliarAtivoInvestimentoAsync(
        string usuarioId,
        PluggyInvestmentAssetReconciliationRequestDTO request,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<IReadOnlyList<PluggyTransactionPreviewDTO>> ListarTransacoesPreviewAsync(
        string usuarioId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyOverviewDTO> ObterVisaoGeralAsync(
        string usuarioId,
        PluggyOverviewFilterDTO filtro,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(
        string usuarioId,
        PluggyTransactionImportRequestDTO request,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyExpenseReconciliationResultDTO> ReconciliarDespesaAsync(
        string usuarioId,
        PluggyExpenseReconciliationRequestDTO request,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task<PluggyInvestmentReconciliationResultDTO> ReconciliarInvestimentoAsync(
        string usuarioId,
        PluggyInvestmentReconciliationRequestDTO request,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);

    Task RemoverConexaoAsync(
        string usuarioId,
        Guid connectionId,
        CancellationToken cancellationToken = default,
        Guid? carteiraId = null);
}
