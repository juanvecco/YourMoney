using YourMoney.Tests.Api;
using YourMoney.Tests.Application;

var tests = new (string Name, Func<Task> Run)[]
{
    ("cent distribution preserves exact total", DespesaServiceParcelamentoTests.CentDistributionPreservesExactTotal),
    ("end-of-month dates clamp predictably", DespesaServiceParcelamentoTests.EndOfMonthDatesClampPredictably),
    ("atomic creation shares group id", DespesaServiceParcelamentoTests.AtomicCreationSharesGroupId),
    ("monthly query includes installment metadata", DespesaServiceParcelamentoTests.MonthlyQueryIncludesInstallmentMetadata),
    ("monthly query keeps regular metadata null", DespesaServiceParcelamentoTests.MonthlyQueryKeepsRegularMetadataNull),
    ("invalid installment quantity fails", DespesaServiceParcelamentoTests.InvalidInstallmentQuantityFails),
    ("invalid value and description fail", DespesaServiceParcelamentoTests.InvalidValueAndDescriptionFail),
    ("missing account or category fails", DespesaServiceParcelamentoTests.MissingAccountOrCategoryFails),
    ("batch failure leaves no partial rows", DespesaServiceParcelamentoTests.BatchFailureLeavesNoPartialRows),
    ("post parcelamento returns created contract", DespesasControllerParcelamentoTests.PostParcelamentoReturnsCreatedContract),
    ("monthly query returns ok contract", DespesasControllerParcelamentoTests.MonthlyQueryReturnsOkContract),
    ("parcelamento endpoint requires authorization", DespesasControllerParcelamentoTests.ParcelamentoEndpointRequiresAuthorization)
};

var failures = new List<string>();

foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failures.Add($"{test.Name}: {ex.Message}");
        Console.WriteLine($"FAIL {test.Name}: {ex.Message}");
    }
}

if (failures.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Failures:");
    foreach (var failure in failures)
        Console.WriteLine($"- {failure}");

    return 1;
}

Console.WriteLine();
Console.WriteLine($"{tests.Length} tests passed.");
return 0;
