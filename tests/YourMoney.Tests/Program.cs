using YourMoney.Tests.Api;
using YourMoney.Tests.Application;
using YourMoney.Tests.Infrastructure;

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
    ("parcelamento endpoint requires authorization", DespesasControllerParcelamentoTests.ParcelamentoEndpointRequiresAuthorization),
    ("post despesa returns created contract", DespesasControllerCreationTests.PostDespesaReturnsCreatedContract),
    ("post despesa endpoint requires authorization", DespesasControllerCreationTests.PostDespesaEndpointRequiresAuthorization),
    ("expense create uses current user", DespesaCreationTests.CreatesExpenseFromTypedRequestWithCurrentUser),
    ("expense create rejects missing description", DespesaCreationTests.RejectsMissingDescription),
    ("expense create rejects invalid value", DespesaCreationTests.RejectsInvalidValue),
    ("expense create rejects invalid reference month", DespesaCreationTests.RejectsInvalidReferenceMonth),
    ("expense create rejects foreign conta", DespesaCreationTests.RejectsForeignConta),
    ("expense create rejects foreign categoria", DespesaCreationTests.RejectsForeignCategoria),
    ("expense create isolation keeps owner scope", DespesaCreationIsolationTests.CreatedExpenseIsListedOnlyForAuthenticatedOwner),
    ("financial ownership scripts contain target user constants", FinancialOwnershipScriptTests.ScriptsContainTargetUserConstants),
    ("financial ownership backfill stops when target user is missing", FinancialOwnershipScriptTests.BackfillStopsWhenTargetUserIsMissing),
    ("financial ownership backfill checks UsuarioId columns before writing", FinancialOwnershipScriptTests.BackfillChecksUsuarioIdColumnsBeforeWriting),
    ("financial ownership backfill updates only ownership columns", FinancialOwnershipScriptTests.BackfillUpdatesOnlyOwnershipColumns),
    ("financial ownership backfill is idempotent", FinancialOwnershipScriptTests.BackfillIsIdempotent),
    ("financial ownership backfill uses transaction and dry-run rollback", FinancialOwnershipScriptTests.BackfillUsesTransactionAndDryRunRollback),
    ("financial ownership validation detects invalid owners", FinancialOwnershipScriptTests.ValidationDetectsInvalidOwners),
    ("financial ownership validation detects relationship mismatches", FinancialOwnershipScriptTests.ValidationDetectsRelationshipMismatches),
    ("financial ownership validation reports financial totals and dates", FinancialOwnershipScriptTests.ValidationReportsFinancialTotalsAndDates),
    ("financial ownership runner supports validate and execute modes", FinancialOwnershipScriptTests.RunnerSupportsValidateAndExecuteModes),
    ("financial repositories filter reads by usuario id", FinancialRepositoryIsolationTests.RepositoriesFilterReadsByUsuarioId),
    ("financial repositories include usuario id in period and list queries", FinancialRepositoryIsolationTests.PeriodAndListQueriesIncludeUsuarioId),
    ("financial services use current user", FinancialServiceIsolationTests.ServicesUseCurrentUserForFinancialOperations),
    ("financial services do not expose client supplied owners", FinancialServiceIsolationTests.ServicesDoNotExposeClientSuppliedOwners),
    ("dashboard uses current user for all aggregations", DashboardIsolationTests.DashboardUsesCurrentUserForAllAggregations),
    ("dashboard does not use unscoped repository calls", DashboardIsolationTests.DashboardDoesNotUseUnscopedRepositoryCalls),
    ("financial controllers require authorization", FinancialControllerAuthorizationTests.FinancialControllersRequireAuthorization),
    ("current user service resolves supported user id claims", CurrentUserServiceTests.ResolvesUserIdFromSupportedJwtClaims),
    ("current user service fails when user id is missing", CurrentUserServiceTests.ThrowsWhenAuthenticatedUserIdIsMissing),
    ("financial queries use authenticated user after login", FinancialDataAfterLoginTests.FinancialQueriesUseAuthenticatedUserForPeriodData),
    ("financial reads do not mutate ownership", FinancialDataAfterLoginTests.FinancialServicesDoNotMutateExistingOwnershipForReads),
    ("financial data after login controllers stay authorized", FinancialDataAfterLoginAuthorizationTests.FinancialControllersStayProtectedAfterLoginFix),
    ("financial controllers stay protected for local frontend origins", FinancialDataAfterLoginAuthorizationTests.FinancialControllersStayProtectedForLocalFrontendOrigins),
    ("startup script opens hosted dashboard as primary frontend url", StaticApplicationHostingTests.StartupScriptOpensHostedDashboardAsPrimaryFrontendUrl),
    ("startup script prints same frontend url it opens", StaticApplicationHostingTests.StartupScriptPrintsTheSameFrontendUrlItOpens),
    ("cors allows local development origins without credential sharing", StaticApplicationHostingTests.CorsAllowsLocalDevelopmentOriginsWithoutCredentialSharing),
    ("api serves angular spa with route fallback", StaticApplicationHostingTests.ApiConfigurationServesAngularSpaWithRouteFallback),
    ("published entry point is angular app with bearer interceptor", PublishedAngularAppTests.PublishedEntryPointIsAngularAppWithBearerInterceptor)
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
