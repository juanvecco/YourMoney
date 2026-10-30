using System.Reflection;
using YourMoney.Api.Services;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class PluggyInvestmentReconciliationTests
{
    private static readonly MethodInfo EligibilityMethod = typeof(PluggyService).GetMethod(
        "PodeReconciliarComoInvestimento",
        BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Investment transaction eligibility rule was not found.");

    public static Task OnlyFinalizedBrlDebitsAreEligible()
    {
        TestAssert.True(Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "DEBIT", Status = "POSTED", Amount = -20.50m,
            CurrencyCode = "BRL", AccountCurrencyCode = "BRL"
        }), "A posted BRL debit should be eligible for explicit review");
        TestAssert.True(!Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "CREDIT", Status = "POSTED", Amount = 20m,
            CurrencyCode = "BRL", AccountCurrencyCode = "BRL"
        }), "A credit must not be classified as an investment purchase");
        TestAssert.True(!Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "DEBIT", Status = "PENDING", Amount = -20m,
            CurrencyCode = "BRL", AccountCurrencyCode = "BRL"
        }), "A pending debit must not be eligible");
        TestAssert.True(!Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "DEBIT", Status = "AUTHORIZED", Amount = -20m,
            CurrencyCode = "BRL", AccountCurrencyCode = "BRL"
        }), "An authorization without a finalized posted status must not be eligible");
        TestAssert.True(!Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "DEBIT", Status = "POSTED", OperationType = "TRANSFER",
            Amount = -20m, CurrencyCode = "BRL", AccountCurrencyCode = "BRL"
        }), "A transfer must remain unavailable");
        TestAssert.True(!Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "DEBIT", Status = "POSTED", Amount = -20.501m,
            CurrencyCode = "BRL", AccountCurrencyCode = "BRL"
        }), "A source value with fractions smaller than cents must not be rounded silently");
        return Task.CompletedTask;
    }

    public static Task ForeignCurrencyWithoutTrustedBrlAmountIsNotEligible()
    {
        TestAssert.True(!Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "DEBIT", Status = "POSTED", Amount = -20m,
            CurrencyCode = "USD", AccountCurrencyCode = "USD"
        }), "A foreign-currency transaction without a trusted BRL amount must be unavailable");
        TestAssert.True(Elegivel(new PluggyTransactionPreviewDTO
        {
            Direction = "DEBIT", Status = "POSTED", Amount = -20m,
            CurrencyCode = "USD", AmountInAccountCurrency = 100m,
            AccountCurrencyCode = "BRL"
        }), "A provider-supplied BRL equivalent on a BRL account may be reviewed");
        return Task.CompletedTask;
    }

    public static Task ReconciliationWritesPositionAndSourceAtomically()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var start = source.IndexOf(
            "public async Task<PluggyInvestmentReconciliationResultDTO> ReconciliarInvestimentoAsync",
            StringComparison.Ordinal);
        var end = source.IndexOf(
            "private static bool CandidatoCorrespondeAoSnapshot",
            start,
            StringComparison.Ordinal);
        TestAssert.True(start >= 0 && end > start, "The investment reconciliation method should be present");
        var method = source[start..end];
        TestAssert.True(method.Contains("IsolationLevel.Serializable", StringComparison.Ordinal),
            "Concurrent investment reconciliation should serialize source ownership");
        TestAssert.True(method.Contains("CriarInvestimentoAsync(request.Investment!)", StringComparison.Ordinal),
            "Create should use the existing investment service");
        TestAssert.True(method.Contains("AtualizarDadosReconciliadosAsync(", StringComparison.Ordinal),
            "Replace should use the restricted investment update method");
        TestAssert.True(method.Contains("CandidatoCorrespondeAoSnapshot(", StringComparison.Ordinal)
            && method.Contains("GetInvestimentoByIdAsync(", StringComparison.Ordinal),
            "The target and its reviewed snapshot should be revalidated for the current owner");
        TestAssert.True(method.Contains("request.ConfirmPossibleDuplicate", StringComparison.Ordinal)
            && method.Contains("request.Investment.OperacaoId == Guid.Empty", StringComparison.Ordinal),
            "Create should require valid operation identity and duplicate confirmation");
        TestAssert.True(method.Contains("_dbContext.PluggyTransactionImports.Add(new PluggyTransactionImport(", StringComparison.Ordinal),
            "The source link should be part of the same transaction");
        TestAssert.True(method.Contains("mesmaCriacao", StringComparison.Ordinal)
            && method.Contains("existingSource.TransactionId", StringComparison.Ordinal),
            "A retry should recognize the previously linked source and operation");
        TestAssert.True(method.Contains("CommitAsync(cancellationToken)", StringComparison.Ordinal)
            && method.Contains("RollbackAsync(cancellationToken)", StringComparison.Ordinal),
            "The operation should commit both writes or roll them back");
        return Task.CompletedTask;
    }

    public static Task ReplacementChangesOnlyBankSharedFields()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Application", "Services", "InvestimentoService.cs"));
        var start = source.IndexOf("AtualizarDadosReconciliadosAsync(", StringComparison.Ordinal);
        var end = source.IndexOf("GetInvestimentoByIdAsync(", start, StringComparison.Ordinal);
        TestAssert.True(start >= 0 && end > start, "The restricted investment replacement method should exist");
        var method = source[start..end];
        TestAssert.True(method.Contains("AtualizarDescricao(descricao)", StringComparison.Ordinal)
            && method.Contains("AtualizarData(data)", StringComparison.Ordinal)
            && method.Contains("AtualizarValorAtual(valorAtual)", StringComparison.Ordinal),
            "Replacement should update description, civil date and current value");
        TestAssert.True(!method.Contains("AtualizarQuantidade", StringComparison.Ordinal)
            && !method.Contains("AtualizarPrecoMedio", StringComparison.Ordinal)
            && !method.Contains("GrupoObjetivo", StringComparison.Ordinal)
            && !method.Contains("ReceitaRecorrente", StringComparison.Ordinal),
            "Replacement should preserve asset units, average price and classifications");
        return Task.CompletedTask;
    }

    public static Task AssetReconciliationUsesSnapshotAndWritesLinkAtomically()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var start = source.IndexOf("ReconciliarAtivoInvestimentoAsync(", StringComparison.Ordinal);
        var end = source.IndexOf("private async Task<IReadOnlyList<PluggyInvestmentAssetDTO>>", start, StringComparison.Ordinal);
        TestAssert.True(start >= 0 && end > start, "A separate asset reconciliation method should exist");
        var method = source[start..end];
        TestAssert.True(method.Contains("IsolationLevel.Serializable", StringComparison.Ordinal)
            && method.Contains("SnapshotToken", StringComparison.Ordinal)
            && method.Contains("entry.IsCurrent", StringComparison.Ordinal),
            "Reconciliation should serialize and validate the owner-owned current snapshot");
        TestAssert.True(method.Contains("PluggyInvestmentAssetLinks", StringComparison.Ordinal)
            && method.Contains("currentLink", StringComparison.Ordinal)
            && method.Contains("CommitAsync", StringComparison.Ordinal),
            "Reconciliation should be idempotent and atomically persist the unique source link");
        TestAssert.True(method.Contains("snapshot_stale", StringComparison.Ordinal)
            && method.Contains("aggregate_position", StringComparison.Ordinal)
            && !method.Contains("ListarTransacoes", StringComparison.Ordinal),
            "Only fresh investment positions may be written and bank transactions must stay out of this path");
        return Task.CompletedTask;
    }

    public static Task InvestmentAssetQuantitiesAndQuotesKeepEightDecimals()
    {
        var config = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Infrastructure", "Configurations", "InvestimentoConfiguration.cs"));
        var service = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Application", "Services", "InvestimentoService.cs"));
        TestAssert.True(config.Contains("Quantidade", StringComparison.Ordinal)
            && config.Contains("PrecoMedio", StringComparison.Ordinal)
            && config.Contains("PrecoUnitarioAtual", StringComparison.Ordinal)
            && config.Split("decimal(18,8)", StringSplitOptions.None).Length - 1 >= 3,
            "Quantity, average price and current unit quote must preserve eight decimals");
        TestAssert.True(service.Contains("NormalizarPrecisaoAtivo", StringComparison.Ordinal),
            "Application writes should preserve the declared asset precision");
        return Task.CompletedTask;
    }

    private static bool Elegivel(PluggyTransactionPreviewDTO transaction) =>
        (bool)(EligibilityMethod.Invoke(null, new object?[] { transaction }) ?? false);
}
