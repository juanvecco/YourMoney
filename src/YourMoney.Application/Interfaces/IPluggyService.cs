using YourMoney.Application.DTOs;

namespace YourMoney.Application.Interfaces;

public interface IPluggyService
{
    Task<PluggyCredentialStatusDTO> ObterConfiguracaoCredenciaisAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);

    Task<PluggyCredentialStatusDTO> SalvarConfiguracaoCredenciaisAsync(
        string usuarioId,
        PluggyCredentialRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<PluggyConnectTokenResponseDTO> CriarConnectTokenAsync(
        string clientUserId,
        Guid? connectionId,
        CancellationToken cancellationToken = default);

    Task<PluggyConnectionDTO> RegistrarConexaoAsync(
        string usuarioId,
        string itemId,
        CancellationToken cancellationToken = default);

    Task<PluggyConnectionDTO> RegistrarConexaoAsync(
        string usuarioId,
        string itemId,
        Guid? connectionId,
        CancellationToken cancellationToken = default) =>
        RegistrarConexaoAsync(usuarioId, itemId, cancellationToken);

    Task<IReadOnlyList<PluggyConnectionDTO>> ListarConexoesAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PluggyAccountDTO>> ListarContasAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);

    Task<PluggyInvestmentAssetsOverviewDTO> ObterAtivosInvestimentoAsync(
        string usuarioId,
        Guid? connectionId = null,
        bool retryFailed = false,
        CancellationToken cancellationToken = default);

    Task<PluggyInvestmentAssetReconciliationResultDTO> ReconciliarAtivoInvestimentoAsync(
        string usuarioId,
        PluggyInvestmentAssetReconciliationRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PluggyTransactionPreviewDTO>> ListarTransacoesPreviewAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);

    Task<PluggyOverviewDTO> ObterVisaoGeralAsync(
        string usuarioId,
        PluggyOverviewFilterDTO filtro,
        CancellationToken cancellationToken = default);

    Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(
        string usuarioId,
        PluggyTransactionImportRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<PluggyExpenseReconciliationResultDTO> ReconciliarDespesaAsync(
        string usuarioId,
        PluggyExpenseReconciliationRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<PluggyInvestmentReconciliationResultDTO> ReconciliarInvestimentoAsync(
        string usuarioId,
        PluggyInvestmentReconciliationRequestDTO request,
        CancellationToken cancellationToken = default);

    Task RemoverConexaoAsync(
        string usuarioId,
        Guid connectionId,
        CancellationToken cancellationToken = default);
}
