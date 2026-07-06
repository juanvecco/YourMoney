using YourMoney.Tests.Api;
using YourMoney.Tests.Application;
using YourMoney.Tests.Domain;
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
    ("list despesa returns reimbursement-aware contract", DespesasControllerCreationTests.ListDespesaReturnsReimbursementAwareContract),
    ("post despesa endpoint requires authorization", DespesasControllerCreationTests.PostDespesaEndpointRequiresAuthorization),
    ("despesa consulta filters by account type nature and combinations", DespesaConsultaTests.FiltersByAccountTypeNatureAndCombinedFilters),
    ("despesa consulta total uses all filtered results before pagination", DespesaConsultaTests.FilteredTotalUsesAllResultsBeforePagination),
    ("despesa consulta account distribution uses all filtered results before pagination", DespesaConsultaTests.AccountDistributionUsesAllResultsBeforePagination),
    ("despesa consulta total uses liquid values", DespesaConsultaTests.ReimbursedExpensesContributeLiquidValueToFilteredTotal),
    ("despesa consulta pagination returns metadata", DespesaConsultaTests.PaginationReturnsMetadataAndClampsOutOfRangePage),
    ("despesa consulta empty filters return metadata", DespesaConsultaTests.ValidFiltersWithNoMatchesReturnEmptyMetadata),
    ("despesa consulta rejects invalid filters", DespesaConsultaTests.InvalidFiltersReturnValidationError),
    ("despesa consulta controller returns paged contract", DespesasControllerConsultaTests.ConsultaReturnsTypedPagedContract),
    ("despesa consulta controller returns empty total", DespesasControllerConsultaTests.ConsultaReturnsFilteredTotalForEmptyResponse),
    ("despesa consulta controller returns generic validation", DespesasControllerConsultaTests.ConsultaReturnsGenericValidationError),
    ("despesa repository consulta is scoped and filterable", DespesaRepositoryConsultaTests.RepositoryConsultaIsUserScopedAndFilterable),
    ("expense create uses current user", DespesaCreationTests.CreatesExpenseFromTypedRequestWithCurrentUser),
    ("expense create rejects missing description", DespesaCreationTests.RejectsMissingDescription),
    ("expense create rejects invalid value", DespesaCreationTests.RejectsInvalidValue),
    ("expense create rejects invalid reference month", DespesaCreationTests.RejectsInvalidReferenceMonth),
    ("expense create rejects foreign conta", DespesaCreationTests.RejectsForeignConta),
    ("expense create rejects foreign categoria", DespesaCreationTests.RejectsForeignCategoria),
    ("expense create isolation keeps owner scope", DespesaCreationIsolationTests.CreatedExpenseIsListedOnlyForAuthenticatedOwner),
    ("investment create normalizes values and uses current user", InvestimentoCreationTests.CreatesNormalizedInvestmentForCurrentUser),
    ("investment create rejects invalid data", InvestimentoCreationTests.RejectsInvalidCreateData),
    ("investment create rejects text beyond limits", InvestimentoCreationTests.RejectsTextBeyondLimits),
    ("investment update normalizes reference for owner", InvestimentoCreationTests.UpdatesReferenceForAuthenticatedOwner),
    ("investment create isolation keeps owner scope", InvestimentoCreationIsolationTests.CreatedInvestmentIsListedOnlyForOwner),
    ("investment monthly query uses reference and owner", InvestimentoCreationIsolationTests.MonthlyQueryUsesReferenceAndOwner),
    ("post investment returns created contract", InvestimentoControllerCreationTests.PostReturnsCreatedTypedContract),
    ("put investment returns persisted contract", InvestimentoControllerCreationTests.PutReturnsPersistedContract),
    ("put investment returns safe error contracts", InvestimentoControllerCreationTests.PutReturnsSafeErrorContracts),
    ("post investment returns validation message", InvestimentoControllerCreationTests.PostReturnsValidationMessage),
    ("post investment returns safe unexpected failure", InvestimentoControllerCreationTests.PostReturnsSafeUnexpectedFailure),
    ("post investment endpoint requires authorization", InvestimentoControllerCreationTests.PostRequiresAuthorization),
    ("investment repository uses reference with legacy fallback", InvestimentoRepositoryReferenceTests.RepositoryUsesReferenceWithLegacyFallback),
    ("investment migration is additive", InvestimentoRepositoryReferenceTests.ConfigurationAndMigrationAreAdditive),
    ("receita create normalizes value and preserves dates", ReceitaCreationTests.CreatesNormalizedReceitaForCurrentUser),
    ("receita monthly query uses reference and legacy fallback", ReceitaCreationTests.ListsByReferenceAndFallsBackForLegacyRows),
    ("receita create rejects invalid data", ReceitaCreationTests.RejectsInvalidCreateData),
    ("receita create isolation keeps owner scope", ReceitaCreationIsolationTests.CreatedReceitaIsListedOnlyForOwner),
    ("receita nature defaults to available income", ReceitaNaturezaTests.DefaultsToAvailableIncome),
    ("receita nature validates reimbursement link", ReceitaNaturezaTests.ValidatesReimbursementLink),
    ("receita nature clears expense link when not reimbursement", ReceitaNaturezaTests.ClearsExpenseLinkWhenNotReimbursement),
    ("receita classification creates eligible total", ReceitaClassificationTests.CreatesClassifiedReceitaAndEligibleTotal),
    ("receita classification updates and clears invalid link", ReceitaClassificationTests.UpdatesClassificationAndClearsInvalidLink),
    ("receita classification rejects invalid nature", ReceitaClassificationTests.RejectsInvalidNature),
    ("despesa reimbursement reduces liquid expense", DespesaReembolsoTests.ReimbursementReducesLiquidExpense),
    ("despesa reimbursement rejects value above pending expense", DespesaReembolsoTests.RejectsReimbursementAbovePendingExpense),
    ("despesa reimbursement rejects foreign expense", DespesaReembolsoTests.RejectsReimbursementForDifferentUserExpense),
    ("post receita returns created contract", ReceitasControllerCreationTests.PostReturnsCreatedTypedContract),
    ("post receita preserves classification contract", ReceitasControllerCreationTests.PostPreservesClassificationContract),
    ("put receita returns classified contract", ReceitasControllerCreationTests.PutReturnsClassifiedReceitaContract),
    ("post receita returns validation message", ReceitasControllerCreationTests.PostReturnsValidationMessage),
    ("post receita returns validation for invalid reimbursement", ReceitasControllerCreationTests.PostReturnsValidationForInvalidReimbursement),
    ("post receita returns safe unexpected failure", ReceitasControllerCreationTests.PostReturnsSafeUnexpectedFailure),
    ("post receita endpoint requires authorization", ReceitasControllerCreationTests.PostRequiresAuthorization),
    ("receita repository uses reference with legacy fallback", ReceitaRepositoryReferenceTests.RepositoryUsesReferenceWithLegacyFallback),
    ("receita configuration maps existing nullable date column", ReceitaRepositoryReferenceTests.ConfigurationMapsExistingNullableDateColumn),
    ("receita repository supports eligible revenue and reimbursements", ReceitaRepositoryReferenceTests.RepositorySupportsEligibleRevenueAndReimbursements),
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
    ("startup script publishes angular build before opening hosted frontend", StaticApplicationHostingTests.StartupScriptPublishesAngularBuildBeforeOpeningHostedFrontend),
    ("startup cmd delegates to powershell script", StaticApplicationHostingTests.StartupCmdDelegatesToPowerShellScript),
    ("cors allows local development origins without credential sharing", StaticApplicationHostingTests.CorsAllowsLocalDevelopmentOriginsWithoutCredentialSharing),
    ("api serves angular spa with route fallback", StaticApplicationHostingTests.ApiConfigurationServesAngularSpaWithRouteFallback),
    ("published entry point is angular app with bearer interceptor", PublishedAngularAppTests.PublishedEntryPointIsAngularAppWithBearerInterceptor)
    ,
    ("monthly goal entity normalizes and owns data", MetaMensalTests.CreatesNormalizedOwnedMeta),
    ("monthly goal entity rejects invalid data", MetaMensalTests.RejectsInvalidData),
    ("monthly goal service creates edits deletes and calculates", MetaMensalServiceTests.CreatesEditsDeletesAndCalculatesMeta),
    ("monthly goal service calculates summary", MetaMensalServiceTests.CalculatesMonthlySummary),
    ("monthly goal service calculates goals from eligible revenue", MetaMensalServiceTests.CalculatesGoalsFromEligibleRevenue),
    ("monthly goal service shows exceeded planning alerts", MetaMensalServiceTests.ShowsAlertsForExceededPlanningAndZeroRevenue),
    ("metas controller returns summary contract", MetasControllerTests.GetResumoReturnsTypedContract),
    ("metas controller returns command contracts", MetasControllerTests.PostPutDeleteReturnExpectedContracts),
    ("metas controller returns validation and not found contracts", MetasControllerTests.ReturnsValidationAndNotFoundContracts),
    ("metas controller requires authorization", MetasControllerTests.RequiresAuthorization),
    ("monthly goal repository and configuration stay owner scoped", MetaMensalRepositoryTests.RepositoryAndConfigurationStayOwnerScoped)
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
