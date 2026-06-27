using YourMoney.Application.Services;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Application
{
    public static class OpenFinanceServiceTests
    {
        public static async Task SourcesExposePreviewOnlyReadiness()
        {
            var response = await new OpenFinanceService().ObterFontesAsync();

            TestAssert.Equal("preview-only", response.Readiness.Mode, "OpenFinance should start in preview-only mode");
            TestAssert.True(!response.Readiness.RealDataEnabled, "Real banking data must be disabled");
            TestAssert.True(response.Readiness.NextSteps.Count >= 2, "Readiness should expose future next steps");
            TestAssert.True(response.Sources.Any(s => s.Type == "simulated" && s.Status == "available"), "A simulated source should be available");
            TestAssert.True(response.Sources.Any(s => s.Type == "future-consent" && !s.SupportsTransactionPreview), "Future consent source should not support preview");
        }

        public static async Task PreviewReturnsValidSimulatedTransactions()
        {
            var response = await new OpenFinanceService().ObterPreviewTransacoesAsync(null);

            TestAssert.Equal("mock-transactions", response.SourceId!, "Default preview source should be mock-transactions");
            TestAssert.Equal("preview-only", response.Mode, "Preview response should be preview-only");
            TestAssert.True(response.Items.Count >= 2, "Preview should include sample transactions");
            TestAssert.True(response.Items.All(i => i.Amount > 0), "Preview amounts should be positive");
            TestAssert.True(response.Items.All(i => i.Direction is "inflow" or "outflow"), "Direction should be explicit");
            TestAssert.True(response.Items.Any(i => i.DuplicateRisk), "At least one preview item should flag duplicate risk");
            TestAssert.True(response.Items.All(i => i.ImportStatus == "preview-only"), "Preview items should not become imports");
        }

        public static async Task PreviewRejectsUnsupportedSource()
        {
            await TestAssert.ThrowsAsync<ArgumentException>(
                () => new OpenFinanceService().ObterPreviewTransacoesAsync("real-openfinance"),
                "Unsupported source should fail with validation error");
        }

        public static async Task ReadinessKeepsRealDataDisabled()
        {
            var response = await new OpenFinanceService().ObterFontesAsync();

            TestAssert.Equal("preview-only", response.Readiness.Mode, "Readiness should remain preview-only");
            TestAssert.True(!response.Readiness.RealDataEnabled, "Readiness should not enable real data");
            TestAssert.True(response.Readiness.Summary.Contains("simulados"), "Readiness should mention simulated/public data");
        }
    }
}
