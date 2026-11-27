using System.Reflection;
using YourMoney.Api.Services;
using YourMoney.Application.DTOs;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api;

public static class PluggyTransactionClassificationTests
{
    private static readonly MethodInfo ExpenseEligibility = Method(
        "PodeReconciliarComoDespesa", typeof(PluggyTransactionPreviewDTO), typeof(string));
    private static readonly MethodInfo InvestmentEligibility = Method(
        "PodeReconciliarComoInvestimento", typeof(PluggyTransactionPreviewDTO), typeof(string));

    public static Task ManualReviewRequiresMatchingExplicitDestination()
    {
        var review = Source("REVIEW");
        TestAssert.True(!CanReconcile(ExpenseEligibility, review, null),
            "An uncertain source cannot be created as an expense without an explicit choice");
        TestAssert.True(CanReconcile(ExpenseEligibility, review, "expense"),
            "An uncertain source can proceed only after the person explicitly chooses expense");
        TestAssert.True(!CanReconcile(ExpenseEligibility, review, "investment"),
            "An investment choice cannot authorize the expense reconciliation endpoint");
        TestAssert.True(CanReconcile(InvestmentEligibility, review, "investment"),
            "An uncertain source can proceed as investment only after the matching explicit choice");
        TestAssert.True(!CanReconcile(InvestmentEligibility, review, "expense"),
            "An expense choice cannot authorize the investment reconciliation endpoint");

        var excluded = Source("EXCLUDED");
        TestAssert.True(!CanReconcile(ExpenseEligibility, excluded, "expense"),
            "An excluded card payment or transfer cannot be overridden through expense review");
        TestAssert.True(!CanReconcile(InvestmentEligibility, excluded, "investment"),
            "An excluded card payment or transfer cannot be overridden through investment review");

        var expense = Source("EXPENSE");
        TestAssert.True(CanReconcile(ExpenseEligibility, expense, null),
            "A transaction already classified as consumption can use normal expense reconciliation");
        TestAssert.True(CanReconcile(ExpenseEligibility, expense, "expense"),
            "A pending transaction that became posted during review must retain the explicit expense choice");

        var pendingReview = Source("REVIEW");
        pendingReview.Status = "PENDING";
        TestAssert.True(!CanReconcile(ExpenseEligibility, pendingReview, null),
            "A pending source requires an explicit expense choice");
        TestAssert.True(CanReconcile(ExpenseEligibility, pendingReview, "expense"),
            "An operator can register a pending debit after explicitly confirming it as an expense");
        TestAssert.True(!CanReconcile(InvestmentEligibility, pendingReview, "investment"),
            "Pending investment registration remains unavailable");

        var pendingExcluded = Source("EXCLUDED");
        pendingExcluded.Status = "PENDING";
        TestAssert.True(!CanReconcile(ExpenseEligibility, pendingExcluded, "expense"),
            "Explicit review cannot turn a pending card payment or transfer into an expense");
        return Task.CompletedTask;
    }

    public static Task GenericExpenseImportRevalidatesClassification()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var start = source.IndexOf(
            "public async Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(",
            StringComparison.Ordinal);
        var end = source.IndexOf(
            "public async Task<PluggyExpenseReconciliationResultDTO> ReconciliarDespesaAsync(",
            start,
            StringComparison.Ordinal);
        TestAssert.True(start >= 0 && end > start, "The generic Open Finance import method should be present");
        var method = source[start..end];
        TestAssert.True(method.Contains("request.RecordType == \"expense\"", StringComparison.Ordinal)
            && method.Contains("PodeReconciliarComoDespesa(source, request.ReviewedClassification)", StringComparison.Ordinal),
            "Generic expense import should revalidate the classification and explicit review choice on the server");
        return Task.CompletedTask;
    }

    public static Task PreviewDoesNotWriteAndKeepsExistingSourceLinks()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var previewStart = source.IndexOf(
            "private async Task<IReadOnlyList<PluggyTransactionPreviewDTO>> ListarTransacoesParaContasAsync(",
            StringComparison.Ordinal);
        var importStart = source.IndexOf(
            "public async Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(",
            previewStart,
            StringComparison.Ordinal);
        TestAssert.True(previewStart >= 0 && importStart > previewStart,
            "The transaction preview mapper should be available");
        var previewMapper = source[previewStart..importStart];
        TestAssert.True(previewMapper.Contains("Classification = classification.Classification", StringComparison.Ordinal)
            && previewMapper.Contains("transaction.Description", StringComparison.Ordinal)
            && previewMapper.Contains("Category = transaction.Category", StringComparison.Ordinal)
            && !previewMapper.Contains("SaveChanges", StringComparison.Ordinal)
            && !previewMapper.Contains(".Remove(", StringComparison.Ordinal),
            "Category and classification should remain a read-only projection of provider data");

        var registeredStart = source.IndexOf(
            "private async Task AnexarSituacaoCadastroAsync(",
            StringComparison.Ordinal);
        var registeredEnd = source.IndexOf(
            "private static IReadOnlyList<string>? ObterMotivosCorrespondencia(",
            registeredStart,
            StringComparison.Ordinal);
        TestAssert.True(registeredStart >= 0 && registeredEnd > registeredStart,
            "The existing registration projection should be available");
        var registrationProjection = source[registeredStart..registeredEnd];
        TestAssert.True(registrationProjection.Contains("source.RegistrationStatus = \"REGISTERED\"", StringComparison.Ordinal)
            && registrationProjection.Contains("source.RegisteredRecordId = link.RecordId", StringComparison.Ordinal)
            && registrationProjection.Contains("continue;", StringComparison.Ordinal),
            "A previously registered source should retain its original link and remain unavailable for duplicate creation");
        return Task.CompletedTask;
    }

    public static Task NewCategoryIsSavedInsideExpenseImportTransaction()
    {
        var source = File.ReadAllText(RepositoryTestPaths.InYourMoney(
            "src", "YourMoney.Api", "Services", "PluggyService.cs"));
        var start = source.IndexOf(
            "public async Task<PluggyTransactionImportResultDTO> ImportarTransacaoAsync(",
            StringComparison.Ordinal);
        var end = source.IndexOf(
            "public async Task<PluggyExpenseReconciliationResultDTO> ReconciliarDespesaAsync(",
            start,
            StringComparison.Ordinal);
        TestAssert.True(start >= 0 && end > start, "The expense import transaction should be present");
        var method = source[start..end];
        var transactionStart = method.IndexOf("BeginTransactionAsync", StringComparison.Ordinal);
        var resolveCategory = method.IndexOf("PluggyExpenseCategoryResolver.Resolve", StringComparison.Ordinal);
        var addCategory = method.IndexOf("_categoriaService.AdicionarAsync", StringComparison.Ordinal);
        var createExpense = method.IndexOf("_despesaService.CriarDespesaAsync", StringComparison.Ordinal);
        var addImport = method.IndexOf("_dbContext.PluggyTransactionImports.Add", StringComparison.Ordinal);
        var commit = method.IndexOf("CommitAsync", StringComparison.Ordinal);
        TestAssert.True(transactionStart >= 0 && resolveCategory > transactionStart && addCategory > resolveCategory
            && createExpense > addCategory && addImport > createExpense && commit > addImport,
            "Category creation, expense creation, and source linking must be ordered inside one transaction");
        return Task.CompletedTask;
    }

    private static PluggyTransactionPreviewDTO Source(string classification) => new()
    {
        Direction = "DEBIT",
        Status = "POSTED",
        Classification = classification,
        Importable = true,
        Amount = 50m,
        CurrencyCode = "BRL",
        AccountCurrencyCode = "BRL"
    };

    private static bool CanReconcile(MethodInfo method, PluggyTransactionPreviewDTO source, string? reviewedAs) =>
        (bool)(method.Invoke(null, [source, reviewedAs]) ?? false);

    private static MethodInfo Method(string name, params Type[] parameters) =>
        typeof(PluggyService).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: parameters,
            modifiers: null)
        ?? throw new InvalidOperationException($"Eligibility method '{name}' was not found.");
}
