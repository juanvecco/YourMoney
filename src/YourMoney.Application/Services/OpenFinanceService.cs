using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;

namespace YourMoney.Application.Services
{
    public class OpenFinanceService : IOpenFinanceService
    {
        private const string PreviewMode = "preview-only";
        private const string MockTransactionsSourceId = "mock-transactions";
        private const string RealOpenFinanceSourceId = "real-openfinance";

        public Task<OpenFinanceSourcesResponseDTO> ObterFontesAsync()
        {
            var checkedAt = DateTimeOffset.UtcNow;

            var response = new OpenFinanceSourcesResponseDTO
            {
                Readiness = CriarReadiness(checkedAt),
                Sources = new List<OpenFinanceSourceDTO>
                {
                    new()
                    {
                        Id = MockTransactionsSourceId,
                        Name = "OpenFinance mock transactions",
                        Type = "simulated",
                        Status = "available",
                        LastCheckedAt = checkedAt,
                        Message = "Transacoes simuladas disponiveis para preview.",
                        SupportsTransactionPreview = true
                    },
                    new()
                    {
                        Id = "open-finance-public-data",
                        Name = "Open Finance dados abertos",
                        Type = "public",
                        Status = "not-configured",
                        LastCheckedAt = checkedAt,
                        Message = "Dados publicos podem ser conectados em uma etapa futura.",
                        SupportsTransactionPreview = false
                    },
                    new()
                    {
                        Id = RealOpenFinanceSourceId,
                        Name = "Consentimento Open Finance real",
                        Type = "future-consent",
                        Status = "not-configured",
                        Message = "Dados bancarios reais exigem integracao consentida futura.",
                        SupportsTransactionPreview = false
                    }
                }
            };

            return Task.FromResult(response);
        }

        public Task<OpenFinanceTransactionPreviewResponseDTO> ObterPreviewTransacoesAsync(string? sourceId)
        {
            var normalizedSourceId = string.IsNullOrWhiteSpace(sourceId)
                ? MockTransactionsSourceId
                : sourceId.Trim();

            if (!string.Equals(normalizedSourceId, MockTransactionsSourceId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("A fonte informada nao suporta preview de transacoes.");
            }

            var items = CriarTransacoesPreview();
            return Task.FromResult(new OpenFinanceTransactionPreviewResponseDTO
            {
                SourceId = MockTransactionsSourceId,
                Mode = PreviewMode,
                Items = items
            });
        }

        private static OpenFinanceReadinessDTO CriarReadiness(DateTimeOffset timestamp)
        {
            return new OpenFinanceReadinessDTO
            {
                Mode = PreviewMode,
                RealDataEnabled = false,
                Summary = "OpenFinance esta disponivel somente para dados publicos ou simulados em modo preview.",
                NextSteps = new List<string>
                {
                    "Integracao consentida com dados bancarios reais",
                    "Importacao confirmada pelo usuario",
                    "Deteccao de duplicidade contra o historico persistido"
                },
                LastUpdatedAt = timestamp
            };
        }

        private static List<OpenFinanceTransactionPreviewDTO> CriarTransacoesPreview()
        {
            return new List<OpenFinanceTransactionPreviewDTO>
            {
                new()
                {
                    Id = "mock-001",
                    SourceId = MockTransactionsSourceId,
                    ExternalReference = "ext-2026-06-001",
                    TransactionDate = new DateOnly(2026, 6, 5),
                    Description = "Salario mensal",
                    Amount = 5000.00m,
                    Direction = "inflow",
                    SuggestedFinancialType = "receita",
                    DuplicateRisk = false,
                    ImportStatus = PreviewMode
                },
                new()
                {
                    Id = "mock-002",
                    SourceId = MockTransactionsSourceId,
                    ExternalReference = "ext-2026-06-002",
                    TransactionDate = new DateOnly(2026, 6, 6),
                    Description = "Supermercado",
                    Amount = 248.75m,
                    Direction = "outflow",
                    SuggestedFinancialType = "despesa",
                    DuplicateRisk = true,
                    DuplicateReason = "Transacao similar aparece mais de uma vez no preview simulado.",
                    ImportStatus = PreviewMode
                },
                new()
                {
                    Id = "mock-003",
                    SourceId = MockTransactionsSourceId,
                    ExternalReference = "ext-2026-06-003",
                    TransactionDate = new DateOnly(2026, 6, 10),
                    Description = "Transferencia recebida",
                    Amount = 350.00m,
                    Direction = "inflow",
                    SuggestedFinancialType = "unknown",
                    DuplicateRisk = false,
                    ImportStatus = PreviewMode
                }
            };
        }
    }
}
