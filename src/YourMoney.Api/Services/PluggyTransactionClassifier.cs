using System.Globalization;
using System.Text;

namespace YourMoney.Api.Services;

public sealed class PluggyTransactionClassificationInput
{
    public string? Type { get; init; }
    public string? Status { get; init; }
    public string? OperationType { get; init; }
    public string? OperationTypeAdditionalInfo { get; init; }
    public string? Category { get; init; }
    public string? CategoryId { get; init; }
    public string? Description { get; init; }
    public string? AccountType { get; init; }
    public string? MerchantName { get; init; }
    public string? MerchantCategory { get; init; }
    public string? CreditCardBillId { get; init; }
    public string? PaymentMethod { get; init; }
    public string? PaymentReason { get; init; }
    public string? PaymentReceiver { get; init; }
}

public sealed record PluggyTransactionClassificationResult(string Classification, string Reason);

public static class PluggyTransactionClassifier
{
    public static bool IsAvailableInTarget(string classification, string? targetType)
    {
        var target = Normalize(targetType);
        var result = Normalize(classification);
        return target switch
        {
            "INVESTMENT" => result is "INVESTMENT" or "REVIEW",
            "EXPENSE" => result is "EXPENSE" or "REVIEW",
            _ => true
        };
    }

    public static PluggyTransactionClassificationResult Classify(PluggyTransactionClassificationInput source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var type = Normalize(source.Type);
        var status = Normalize(source.Status);
        var operation = Normalize(string.Join(' ', source.OperationType, source.OperationTypeAdditionalInfo));
        var category = Normalize(string.Join(' ', source.Category, source.CategoryId));
        var description = Normalize(source.Description);
        var accountType = Normalize(source.AccountType);
        var paymentContext = Normalize(string.Join(' ', source.PaymentMethod, source.PaymentReason, source.PaymentReceiver));
        var structuredContext = Normalize(string.Join(' ', operation, category, accountType, paymentContext));

        if (status == "PENDING" || status == "AUTHORIZED" || status == "AUTHORISED")
            return Excluded("UNSUPPORTED_STATUS");

        if (string.IsNullOrWhiteSpace(status))
            return Review("UNSUPPORTED_STATUS");

        var cardBill = IsCardBillPayment(
            source,
            operation,
            category,
            description,
            accountType,
            paymentContext,
            Normalize(source.MerchantCategory));
        var transfer = HasAny(structuredContext, "TRANSFER", "TRANSFERENCIA", "INTERNAL TRANSFER")
            || HasAny(description, "TRANSFERENCIA ENTRE CONTAS", "TRANSFERENCIA PARA CORRETORA", "TRANSFER TO BROKER");
        var pixMayBeTransfer = HasAny(description, "PIX ENVIADO", "PIX TRANSFERIDO", "PIX TRANSFER");
        var redemption = HasAny(structuredContext, "RESGATE", "REDEMPTION", "INVESTMENT WITHDRAWAL")
            || HasAny(description, "RESGATE", "REDEMPTION");
        var reversal = HasAny(structuredContext, "ESTORNO", "REVERSAL", "REFUND", "CASHBACK")
            || HasAny(description, "ESTORNO", "REVERSAL", "REFUND");

        var investmentEvidence = IsInvestmentAcquisition(operation, category, description);
        var expenseEvidence = IsConsumption(operation, category, source.MerchantCategory, description);

        if ((cardBill || transfer || reversal || pixMayBeTransfer) && (investmentEvidence || expenseEvidence))
            return Review("CONFLICTING_SIGNALS");

        if (cardBill)
            return Excluded("CARD_BILL_PAYMENT");
        if (transfer)
            return Excluded("TRANSFER");
        if (redemption)
            return Excluded("REDEMPTION");
        if (reversal)
            return Excluded("REVERSAL");
        if (pixMayBeTransfer)
            return Review("AMBIGUOUS");

        if (status != "POSTED")
            return Review("UNSUPPORTED_STATUS");

        if (type == "CREDIT")
            return Excluded("NON_CONSUMPTION");
        if (type != "DEBIT")
            return Review("AMBIGUOUS");

        if (investmentEvidence && expenseEvidence)
            return Review("CONFLICTING_SIGNALS");
        if (investmentEvidence)
            return Investment();
        if (expenseEvidence)
            return Expense();

        return Review("AMBIGUOUS");
    }

    private static bool IsCardBillPayment(
        PluggyTransactionClassificationInput source,
        string operation,
        string category,
        string description,
        string accountType,
        string paymentContext,
        string merchantCategory)
    {
        var structured = Normalize(string.Join(' ', operation, category, paymentContext));
        if (HasAny(structured, "PAGAMENTO FATURA", "PAGAMENTO DE FATURA", "PAGAMENTO CARTAO FATURA", "CARD BILL PAYMENT", "CREDIT CARD BILL PAYMENT"))
            return true;

        var hasPayment = HasToken(description, "PAGAMENTO", "PGTO", "PAYMENT");
        var hasBill = HasToken(description, "FATURA", "FAT", "BILL");
        var hasCardContext = HasAny(description, "CARTAO", "CARD")
            || HasAny(accountType, "CREDIT CARD", "CREDITCARD", "CARTAO CREDITO")
            || HasAny(category, "CREDIT CARD", "CARTAO CREDITO")
            || (source.CreditCardBillId is not null && HasAny(paymentContext, "FATURA", "BILL"));

        if (hasPayment && hasBill
            && !hasCardContext
            && (IsUtilityCategory(category) || IsUtilityCategory(merchantCategory)))
            return false;

        // A complete payment-plus-bill phrase is meaningful even when the institution omits the card name.
        if (hasPayment && hasBill)
            return true;

        var operationPayment = HasAny(operation, "PAGAMENTO", "PGTO", "PAYMENT");
        var paymentDescribesBill = HasAny(paymentContext, "FATURA", "BILL");
        return operationPayment && hasCardContext && paymentDescribesBill;
    }

    private static bool IsInvestmentAcquisition(string operation, string category, string description)
    {
        var investmentCategory = HasAny(category,
            "INVESTMENT", "INVESTIMENTO", "FIXED INCOME", "RENDA FIXA", "TESOURO DIRETO");
        var namedAsset = HasAny(description, "RDB", "CDB", "TESOURO DIRETO", "TITULO PUBLICO");
        var descriptionHasApplication = HasAny(description,
            "APLICACAO", "APLICACAO FINANCEIRA", "INVESTMENT APPLICATION");
        var applicationOperation = HasAny(operation,
            "APLICACAO", "INVESTMENT APPLICATION", "APLICACAO FINANCEIRA", "INVESTIMENTO");
        var purchaseOperation = IsPurchase(operation) || HasAny(description, "COMPRA", "PURCHASE");
        var applicationWithNamedAsset = (applicationOperation || descriptionHasApplication) && namedAsset;

        if (applicationWithNamedAsset || (applicationOperation && investmentCategory))
            return true;
        if (investmentCategory && purchaseOperation)
            return true;
        return namedAsset && purchaseOperation;
    }

    private static bool IsConsumption(string operation, string category, string? merchantCategory, string description)
    {
        var operationHasPurchase = IsPurchase(operation);
        var categoryIsInvestment = HasAny(category, "INVESTMENT", "INVESTIMENTO", "FIXED INCOME", "RENDA FIXA");
        var categoryIsConsumption = IsConsumerCategory(category) || IsConsumerCategory(Normalize(merchantCategory));
        var descriptionHasPurchase = HasAny(description, "COMPRA", "PURCHASE");

        if (categoryIsInvestment && !categoryIsConsumption)
            return categoryIsConsumption;
        return operationHasPurchase || categoryIsConsumption || (descriptionHasPurchase && !categoryIsInvestment);
    }

    private static bool IsPurchase(string value) => HasAny(value,
        "PURCHASE", "PURCHASES", "CARD PURCHASE", "CARD PURCHASES", "COMPRA", "COMPRAS", "COMPRA CARTAO");

    private static bool IsConsumerCategory(string value) => HasAny(value,
        "GROCERY", "GROCERIES", "SUPERMARKET", "MERCADO", "FOOD", "RESTAURANT", "RESTAURANTE",
        "PHARMACY", "FARMACIA", "TRANSPORT", "TRANSPORTE", "SHOPPING", "RETAIL", "COMERCIO",
        "HEALTH", "SAUDE", "EDUCATION", "EDUCACAO", "LEISURE", "LAZER", "TRAVEL", "VIAGEM",
        "UTILITIES", "UTILITY", "SERVICES", "SERVICOS", "SUBSCRIPTION", "ASSINATURA", "INSURANCE", "SEGURO");

    private static bool IsUtilityCategory(string value) => HasAny(value,
        "UTILITIES", "UTILITY", "ENERGY", "ELECTRICITY", "WATER", "GAS", "INTERNET", "TELECOMMUNICATION");

    private static bool HasAny(string value, params string[] patterns) =>
        patterns.Any(pattern => value.Contains(Normalize(pattern), StringComparison.Ordinal));

    private static bool HasToken(string value, params string[] tokens)
    {
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Any(token => words.Contains(Normalize(token), StringComparer.Ordinal));
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;
            builder.Append(char.IsLetterOrDigit(character) ? char.ToUpperInvariant(character) : ' ');
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static PluggyTransactionClassificationResult Expense() => new("EXPENSE", "CONSUMPTION");
    private static PluggyTransactionClassificationResult Investment() => new("INVESTMENT", "INVESTMENT_APPLICATION");
    private static PluggyTransactionClassificationResult Excluded(string reason) => new("EXCLUDED", reason);
    private static PluggyTransactionClassificationResult Review(string reason) => new("REVIEW", reason);
}
