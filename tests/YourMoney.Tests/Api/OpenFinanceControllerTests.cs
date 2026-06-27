using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourMoney.Api.Controllers;
using YourMoney.Application.DTOs;
using YourMoney.Application.Interfaces;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Api
{
    public static class OpenFinanceControllerTests
    {
        public static async Task SourcesReturnTypedContract()
        {
            var response = new OpenFinanceSourcesResponseDTO
            {
                Readiness = new OpenFinanceReadinessDTO
                {
                    Mode = "preview-only",
                    RealDataEnabled = false,
                    Summary = "Preview only",
                    NextSteps = new List<string> { "Consentimento futuro" }
                },
                Sources = new List<OpenFinanceSourceDTO>
                {
                    new()
                    {
                        Id = "mock-transactions",
                        Name = "Mock",
                        Type = "simulated",
                        Status = "available",
                        Message = "Disponivel",
                        SupportsTransactionPreview = true
                    }
                }
            };

            var result = await new OpenFinanceController(new FakeOpenFinanceService { SourcesResponse = response }).ObterFontes();

            var ok = result as OkObjectResult;
            TestAssert.True(ok != null, "Sources should return 200 OK");
            TestAssert.Equal(response, ok!.Value, "Sources should return typed response");
            TestAssert.True(!response.Readiness.RealDataEnabled, "Real data must stay disabled");
        }

        public static async Task PreviewReturnsTypedContract()
        {
            var response = new OpenFinanceTransactionPreviewResponseDTO
            {
                SourceId = "mock-transactions",
                Mode = "preview-only",
                Items = new List<OpenFinanceTransactionPreviewDTO>
                {
                    new()
                    {
                        Id = "mock-001",
                        SourceId = "mock-transactions",
                        TransactionDate = new DateOnly(2026, 6, 5),
                        Description = "Salario",
                        Amount = 5000m,
                        Direction = "inflow",
                        SuggestedFinancialType = "receita",
                        DuplicateRisk = false,
                        ImportStatus = "preview-only"
                    }
                }
            };
            var service = new FakeOpenFinanceService { PreviewResponse = response };

            var result = await new OpenFinanceController(service).ObterPreviewTransacoes("mock-transactions");

            var ok = result as OkObjectResult;
            TestAssert.True(ok != null, "Preview should return 200 OK");
            TestAssert.Equal(response, ok!.Value, "Preview should return typed response");
            TestAssert.Equal("mock-transactions", service.LastSourceId!, "Controller should pass sourceId to service");
        }

        public static async Task PreviewReturnsBadRequestForInvalidSource()
        {
            var service = new FakeOpenFinanceService
            {
                PreviewException = new ArgumentException("A fonte informada nao suporta preview de transacoes.")
            };

            var result = await new OpenFinanceController(service).ObterPreviewTransacoes("real-openfinance");

            TestAssert.True(result is BadRequestObjectResult, "Invalid source should return 400");
        }

        public static Task ControllerRequiresAuthorization()
        {
            var hasAuthorize = typeof(OpenFinanceController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            TestAssert.True(hasAuthorize, "OpenFinance controller should require authorization");
            return Task.CompletedTask;
        }

        private sealed class FakeOpenFinanceService : IOpenFinanceService
        {
            public OpenFinanceSourcesResponseDTO SourcesResponse { get; set; } = new();
            public OpenFinanceTransactionPreviewResponseDTO PreviewResponse { get; set; } = new();
            public Exception? SourcesException { get; set; }
            public Exception? PreviewException { get; set; }
            public string? LastSourceId { get; private set; }

            public Task<OpenFinanceSourcesResponseDTO> ObterFontesAsync()
            {
                if (SourcesException != null)
                    throw SourcesException;

                return Task.FromResult(SourcesResponse);
            }

            public Task<OpenFinanceTransactionPreviewResponseDTO> ObterPreviewTransacoesAsync(string? sourceId)
            {
                LastSourceId = sourceId;
                if (PreviewException != null)
                    throw PreviewException;

                return Task.FromResult(PreviewResponse);
            }
        }
    }
}
