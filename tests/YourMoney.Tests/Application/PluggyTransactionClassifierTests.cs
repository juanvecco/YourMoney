using YourMoney.Api.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application;

public static class PluggyTransactionClassifierTests
{
    public static Task RecognizesExpensesAndExcludesCardBillPayments()
    {
        TestAssert.Equal("EXPENSE", Classify().Classification,
            "A completed purchase supported by consumer operation, category and merchant should remain an expense");
        TestAssert.Equal("EXCLUDED", Classify(
            operationType: "PAGAMENTO_FATURA",
            description: "Pagamento da Fatura",
            category: null,
            accountType: "CREDIT_CARD",
            merchantName: null).Classification,
            "A structured card bill payment must not be offered as an expense");
        TestAssert.Equal("EXCLUDED", Classify(
            operationType: "PAGAMENTO",
            description: "PGTO FAT CARTAO C6",
            category: null,
            accountType: "CREDIT_CARD",
            merchantName: null).Classification,
            "The abbreviated C6 card bill description must be excluded");
        TestAssert.Equal("EXCLUDED", Classify(
            operationType: "PAGAMENTO",
            description: "Pagamento da Fatura",
            category: null,
            merchantName: null).Classification,
            "A compound bill-payment phrase must be recognized even if the source omits extra metadata");
        TestAssert.Equal("EXPENSE", Classify(
            operationType: "CARD_PURCHASE",
            category: "Groceries",
            description: "Pagamento Mercado",
            merchantName: "Mercado Central").Classification,
            "Payment wording alone must not hide a purchase supported by purchase and grocery metadata");
        TestAssert.Equal("EXPENSE", Classify(
            operationType: "PAYMENT",
            category: "Utilities",
            description: "Pagamento da Fatura de Energia",
            merchantCategory: "Utilities",
            accountType: "CHECKING").Classification,
            "A utility bill supported by utility category must remain a household expense");
        return Task.CompletedTask;
    }

    public static Task RoutesInvestmentsTransfersAndRedemptionsCorrectly()
    {
        TestAssert.Equal("INVESTMENT", Classify(
            operationType: "APLICACAO",
            category: "Fixed Income Investment",
            description: "Aplicação RDB").Classification,
            "A completed RDB application with investment evidence should be an investment");
        TestAssert.Equal("INVESTMENT", Classify(
            operationType: "PIX",
            category: null,
            description: "Aplicação RDB",
            merchantName: null).Classification,
            "A combined application and RDB description is sufficient investment evidence when structured fields are absent");
        TestAssert.Equal("INVESTMENT", Classify(
            operationType: "PURCHASE",
            category: "Fixed Income Investment",
            description: "Compra Tesouro Direto").Classification,
            "A completed Treasury Direct purchase with investment category should be an investment");
        TestAssert.Equal("EXCLUDED", Classify(
            operationType: "PIX",
            category: "Transfer",
            description: "PIX ENVIADO PARA CORRETORA").Classification,
            "A transfer to a broker without acquisition evidence must not become an investment");
        TestAssert.Equal("REVIEW", Classify(
            operationType: "PIX",
            category: null,
            description: "PIX enviado para uma pessoa").Classification,
            "An outgoing Pix without structured transfer or merchant evidence must not be presumed to be consumption");
        TestAssert.Equal("EXCLUDED", Classify(
            operationType: "RESGATE_APLIC_FINANCEIRA",
            category: "Fixed Income Investment",
            description: "Resgate RDB").Classification,
            "A redemption must not be classified as an investment acquisition");
        TestAssert.Equal("EXCLUDED", Classify(status: "PENDING").Classification,
            "A pending transaction cannot be registered as an expense or investment");
        return Task.CompletedTask;
    }

    public static Task SendsInsufficientOrConflictingEvidenceToReview()
    {
        TestAssert.Equal("REVIEW", Classify(
            operationType: "PIX",
            category: null,
            description: "PIX").Classification,
            "A PIX debit without purchase or transfer evidence must be reviewed");
        TestAssert.Equal("REVIEW", Classify(
            operationType: "CARD_PURCHASE",
            category: "Fixed Income Investment",
            description: "Compra RDB",
            merchantName: "Supermercado Central",
            merchantCategory: "Grocery").Classification,
            "Conflicting purchase and investment evidence must be reviewed");
        TestAssert.Equal("REVIEW", Classify(type: null, operationType: "PURCHASE").Classification,
            "Missing direction cannot be inferred from description or category");
        TestAssert.Equal("EXCLUDED", Classify(type: "CREDIT", operationType: "PURCHASE").Classification,
            "A credit must not be offered as a consumption expense");
        return Task.CompletedTask;
    }

    public static Task KeepsTargetsExclusiveAndReviewVisible()
    {
        TestAssert.True(PluggyTransactionClassifier.IsAvailableInTarget("INVESTMENT", "investment"),
            "An investment must be available in the investment context");
        TestAssert.True(!PluggyTransactionClassifier.IsAvailableInTarget("EXPENSE", "investment"),
            "A consumption expense must not appear in the investment context");
        TestAssert.True(PluggyTransactionClassifier.IsAvailableInTarget("REVIEW", "investment"),
            "An uncertain transaction remains available for a separate review in the investment context");
        TestAssert.True(PluggyTransactionClassifier.IsAvailableInTarget("EXPENSE", "expense"),
            "A consumption expense must be available in the expense context");
        TestAssert.True(!PluggyTransactionClassifier.IsAvailableInTarget("INVESTMENT", "expense"),
            "An investment must not appear in the expense context");
        return Task.CompletedTask;
    }

    public static Task ClassificationPreservesOriginalProviderValues()
    {
        var source = new PluggyTransactionClassificationInput
        {
            Type = "DEBIT",
            Status = "POSTED",
            OperationType = "PIX",
            Category = "Groceries",
            Description = "Mercado de bairro",
            PaymentReason = "Compra presencial"
        };

        _ = PluggyTransactionClassifier.Classify(source);

        TestAssert.Equal("PIX", source.OperationType, "Classification must not rewrite the provider operation type");
        TestAssert.Equal("Groceries", source.Category, "Classification must not rewrite the provider category");
        TestAssert.Equal("Mercado de bairro", source.Description, "Classification must not rewrite the provider description");
        TestAssert.Equal("Compra presencial", source.PaymentReason, "Classification must not rewrite payment evidence");
        return Task.CompletedTask;
    }

    private static PluggyTransactionClassificationResult Classify(
        string? type = "DEBIT",
        string? status = "POSTED",
        string? operationType = "CARD_PURCHASE",
        string? category = "Groceries",
        string? description = "Compra no mercado",
        string? accountType = "CHECKING",
        string? merchantName = "Mercado Central",
        string? merchantCategory = null,
        string? operationTypeAdditionalInfo = null,
        string? cardBillId = null,
        string? paymentMethod = null,
        string? paymentReason = null) =>
        PluggyTransactionClassifier.Classify(new PluggyTransactionClassificationInput
        {
            Type = type,
            Status = status,
            OperationType = operationType,
            OperationTypeAdditionalInfo = operationTypeAdditionalInfo,
            Category = category,
            Description = description,
            AccountType = accountType,
            MerchantName = merchantName,
            MerchantCategory = merchantCategory,
            CreditCardBillId = cardBillId,
            PaymentMethod = paymentMethod,
            PaymentReason = paymentReason
        });
}
