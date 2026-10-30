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
    ("recurring expense entity creates monthly recurrence", DespesaRecorrenteTests.CreatesMonthlyRecurrenceAndClampsDueDate),
    ("recurring expense occurrence finalizes once", DespesaRecorrenteTests.OccurrenceCanBeConfirmedOrIgnoredOnlyOnce),
    ("recurring expense rejects invalid period", DespesaRecorrenteTests.RejectsInvalidRecurrencePeriod),
    ("recurring expense suggestions create pending occurrence", DespesaRecorrenteServiceTests.ListSuggestionsCreatesPendingMonthlyOccurrenceWithoutExpense),
    ("recurring expense suggestion confirmation creates expense", DespesaRecorrenteServiceTests.ConfirmSuggestionCreatesExpenseAndFinalizesOccurrence),
    ("recurring expense ignored suggestion allows next month", DespesaRecorrenteServiceTests.IgnoredSuggestionDoesNotBlockFollowingMonths),
    ("recurring expense rejects invalid category hierarchy", DespesaRecorrenteServiceTests.RejectsInvalidCategoryHierarchy),
    ("recurring expense maintenance preserves finalized occurrences", DespesaRecorrenteServiceTests.UpdatesDeactivatesAndEndsRecurrenceWithoutChangingFinalizedOccurrences),
    ("recurring expenses controller returns created contract", DespesasRecorrentesControllerTests.PostRecurrenceReturnsCreatedContract),
    ("recurring expenses controller confirms suggestion contract", DespesasRecorrentesControllerTests.ConfirmSuggestionReturnsCreatedExpenseContract),
    ("recurring expenses controller returns conflict for finalized suggestion", DespesasRecorrentesControllerTests.FinalizedSuggestionReturnsConflict),
    ("recurring expenses controller maintains recurrence", DespesasRecorrentesControllerTests.PutAndPatchMaintenanceReturnExpectedContracts),
    ("recurring expenses controller requires authorization", DespesasRecorrentesControllerTests.RecurrenceControllerRequiresAuthorization),
    ("recurring expense mapping creates tables", DespesaRecorrenteRepositoryTests.RecurringExpenseMappingCreatesTablesAndUniqueMonthlyOccurrence),
    ("recurring expense repository keeps user scope", DespesaRecorrenteRepositoryTests.RecurringExpenseRepositoryKeepsUserScopedQueries),
    ("recurring revenue entity validates and clamps receiving date", ReceitaRecorrenteTests.CreatesValidRecurrenceAndClampsReceivingDate),
    ("recurring revenue entity rejects invalid data", ReceitaRecorrenteTests.RejectsInvalidDataAndReimbursementNature),
    ("recurring revenue occurrence finalizes once", ReceitaRecorrenteTests.OccurrenceFinalizesOnlyOnce),
    ("recurring revenue maintenance changes eligibility", ReceitaRecorrenteTests.MaintenancePreservesConfiguredPeriod),
    ("recurring revenue create does not create normal revenue", ReceitaRecorrenteServiceTests.CreatesListsAndDoesNotAutomaticallyCreateRevenue),
    ("recurring revenue suggestions materialize and confirm", ReceitaRecorrenteServiceTests.MaterializesAndFinalizesMonthlySuggestions),
    ("recurring revenue ignore affects one month", ReceitaRecorrenteServiceTests.IgnoresOnlySelectedMonth),
    ("recurring revenue reserve projection stays exact and separate", ReceitaRecorrenteServiceTests.CalculatesSeparateExactReserveProjections),
    ("recurring revenue account and flags are validated", ReceitaRecorrenteServiceTests.RejectsForeignAccountAndPersistsIndependentFlags),
    ("recurring revenue maintenance preserves finalized history", ReceitaRecorrenteServiceTests.MaintenanceStopsFutureViewsAndPreservesFinalizedOccurrence),
    ("recurring revenue controller returns CRUD contracts", ReceitasRecorrentesControllerTests.CrudReturnsExpectedContracts),
    ("recurring revenue controller returns suggestion contracts", ReceitasRecorrentesControllerTests.SuggestionsReturnCreatedNoContentAndConflictContracts),
    ("recurring revenue controller returns reserve contracts", ReceitasRecorrentesControllerTests.ProjectionReturnsItemsAndEmptyContract),
    ("recurring revenue controller validates authorization and errors", ReceitasRecorrentesControllerTests.MapsValidationNotFoundAndAuthorization),
    ("recurring revenue mapping creates precise unique tables", ReceitaRecorrenteRepositoryTests.MappingCreatesPreciseTablesAndUniqueOccurrences),
    ("recurring revenue repository keeps user scope", ReceitaRecorrenteRepositoryTests.RepositoryKeepsEveryReadUserScoped),
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
    ("investment reserve entity supports optional salary link", InvestimentoReservaTests.CreatesOptionalSalaryLinkAndOperation),
    ("investment reserve entity changes and removes salary link", InvestimentoReservaTests.ChangesAndRemovesSalaryLink),
    ("investment reserve requires description", InvestimentoReservaTests.RequiresDescriptionForNewWrites),
    ("investment reserve mapping is additive and indexed", InvestimentoReservaRepositoryTests.MappingDefinesOptionalRestrictedRelationshipAndIndexes),
    ("investment reserve queries are set based and owner scoped", InvestimentoReservaRepositoryTests.ConsolidatedQueriesAreSetBasedAndOwnerScoped),
    ("investment reserve consolidates all months and owner scope", InvestimentoReservaServiceTests.ConsolidatesAllMonthsAndKeepsOwnerScope),
    ("investment reserve calculates salary goals", InvestimentoReservaServiceTests.CalculatesSalaryGoalsAndKeepsUnlinkedOnlyInGrandTotal),
    ("investment reserve includes marked salary without aporte", InvestimentoReservaServiceTests.IncludesMarkedSalaryWithoutInvestmentAndPreservesCents),
    ("investment reserve replays operation idempotently", InvestimentoReservaServiceTests.ReplaysSameOperationAndRejectsDifferentPayload),
    ("investment reserve preserves historical link", InvestimentoReservaServiceTests.PreservesHistoricalLinkButPreventsReassociation),
    ("investment reserve rejects foreign salary", InvestimentoReservaServiceTests.RejectsForeignSalary),
    ("investment reserve controller returns consolidated contract", InvestimentoReservaControllerTests.ConsolidatedEndpointReturnsTypedEmptyContract),
    ("investment reserve controller returns idempotency conflict", InvestimentoReservaControllerTests.CreateReturnsConflictForReusedOperation),
    ("investment reserve endpoints remain authorized", InvestimentoReservaControllerTests.EndpointsRemainAuthorized),
    ("receita create normalizes value and preserves dates", ReceitaCreationTests.CreatesNormalizedReceitaForCurrentUser),
    ("receita monthly query uses reference and legacy fallback", ReceitaCreationTests.ListsByReferenceAndFallsBackForLegacyRows),
    ("receita create rejects invalid data", ReceitaCreationTests.RejectsInvalidCreateData),
    ("receita keeps legacy account optional and persists selected account", ReceitaCreationTests.KeepsLegacyAccountOptionalAndPersistsConfirmedAccount),
    ("receita rejects foreign account", ReceitaCreationTests.RejectsAccountOutsideAuthenticatedUser),
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
    ("startup script stops project API before build and migration", StaticApplicationHostingTests.StartupScriptStopsOnlyProjectApiBeforeBuildAndMigration),
    ("startup cmd delegates to powershell script", StaticApplicationHostingTests.StartupCmdDelegatesToPowerShellScript),
    ("cors allows local development origins without credential sharing", StaticApplicationHostingTests.CorsAllowsLocalDevelopmentOriginsWithoutCredentialSharing),
    ("api serves angular spa with route fallback", StaticApplicationHostingTests.ApiConfigurationServesAngularSpaWithRouteFallback),
    ("api fallback returns JSON for missing API routes", StaticApplicationHostingTests.ApiFallbackDoesNotReturnSpaForMissingRoutes),
    ("published entry point is angular app with bearer interceptor", PublishedAngularAppTests.PublishedEntryPointIsAngularAppWithBearerInterceptor)
    ,
    ("pluggy connect token uses authenticated user", PluggyControllerTests.ConnectTokenUsesAuthenticatedUser),
    ("pluggy missing configuration returns unavailable", PluggyControllerTests.MissingConfigurationReturnsServiceUnavailable),
    ("pluggy endpoint requires authorization", PluggyControllerTests.EndpointRequiresAuthorization),
    ("monthly goal entity normalizes and owns data", MetaMensalTests.CreatesNormalizedOwnedMeta),
    ("monthly goal entity rejects invalid data", MetaMensalTests.RejectsInvalidData),
    ("monthly goal entity supports value and mode transitions", MetaMensalTests.SupportsValueAndModeTransitions),
    ("monthly goal entity rejects inconsistent definitions and precision", MetaMensalTests.RejectsInconsistentDefinitionsAndPrecision),
    ("monthly goal service creates edits deletes and calculates", MetaMensalServiceTests.CreatesEditsDeletesAndCalculatesMeta),
    ("monthly goal service calculates summary", MetaMensalServiceTests.CalculatesMonthlySummary),
    ("monthly goal service calculates goals from eligible revenue", MetaMensalServiceTests.CalculatesGoalsFromEligibleRevenue),
    ("monthly goal service shows exceeded planning alerts", MetaMensalServiceTests.ShowsAlertsForExceededPlanningAndZeroRevenue),
    ("monthly goal service creates both modes and recalculates derived fields", MetaMensalServiceTests.CreatesBothModesAndRecalculatesDerivedFields),
    ("monthly goal service edits modes and preserves reference month", MetaMensalServiceTests.EditsModesAndPreservesReferenceMonth),
    ("monthly goal service handles zero-base value goals and safe rename", MetaMensalServiceTests.HandlesZeroBaseValueGoalsAndSafeRename),
    ("monthly goal service rejects invalid payloads and foreign goals", MetaMensalServiceTests.RejectsInvalidDiscriminatedPayloadsAndForeignGoals),
    ("monthly goal service rounds mixed goals and keeps hundred-percent boundary", MetaMensalServiceTests.RoundsMixedGoalsAndKeepsHundredPercentBoundary),
    ("metas controller returns summary contract", MetasControllerTests.GetResumoReturnsTypedContract),
    ("metas controller returns command contracts", MetasControllerTests.PostPutDeleteReturnExpectedContracts),
    ("metas controller returns validation and not found contracts", MetasControllerTests.ReturnsValidationAndNotFoundContracts),
    ("metas controller returns conflict and safe unexpected failure", MetasControllerTests.ReturnsConflictAndSafeUnexpectedFailure),
    ("metas controller requires authorization", MetasControllerTests.RequiresAuthorization),
    ("monthly goal repository and configuration stay owner scoped", MetaMensalRepositoryTests.RepositoryAndConfigurationStayOwnerScoped),
    ("monthly goal migration preserves legacy and protects rollback", MetaMensalRepositoryTests.MigrationPreservesLegacyAndProtectsRollback),
    ("investment organization normalizes and limits group names", InvestimentoOrganizacaoDomainTests.NormalizesAndLimitsGroupNames),
    ("investment organization keeps optional controlled state", InvestimentoOrganizacaoDomainTests.KeepsOrganizationOptionalAndControlled),
    ("investment organization persistence is additive and owner safe", InvestimentoOrganizacaoPersistenceTests.MapsNullableOwnedOrganizationSafely),
    ("investment groups create list and rename for owner", GrupoObjetivoInvestimentoServiceTests.CreatesListsAndRenamesOwnerGroups),
    ("investment groups reject duplicates and isolate owners", GrupoObjetivoInvestimentoServiceTests.RejectsDuplicateAndForeignGroups),
    ("investment objective participates in idempotency", GrupoObjetivoInvestimentoServiceTests.AssociatesGroupAndIncludesItInIdempotency),
    ("investment asset types accept stable codes", InvestimentoTipoAtivoTests.AcceptsEveryStableCodeAndPreservesLegacyType),
    ("investment asset type clears and validates", InvestimentoTipoAtivoTests.ClearsAndRejectsInvalidClassification),
    ("investment organization filters and totals exactly", InvestimentoOrganizacaoConsultaTests.FiltersWithAndAndExactBuckets),
    ("investment organization validates absence and ownership", InvestimentoOrganizacaoConsultaTests.ValidatesContradictionsNullBucketsAndForeignGroup),
    ("investment organization handles planned maximum", InvestimentoOrganizacaoConsultaTests.HandlesPlannedMaximumDeterministically),
    ("used objective requires explicit deletion decision", GrupoObjetivoInvestimentoExclusaoTests.RequiresExplicitDecisionForUsedGroup),
    ("objective transfer preserves investments and money", GrupoObjetivoInvestimentoExclusaoTests.UnlinksOrTransfersWithoutDeletingInvestments),
    ("objective deletion rejects invalid destinations", GrupoObjetivoInvestimentoExclusaoTests.RejectsInvalidSelfAndForeignDestinations),
    ("objective deletion repository is atomic and set based", GrupoObjetivoInvestimentoTransacaoTests.UsesSerializableSetBasedAtomicCommand),
    ("objective deletion repository rolls back injected SQL failure", GrupoObjetivoInvestimentoTransacaoTests.RollsBackInjectedDeleteFailureWhenSqlServerIsConfigured),
    ("objective group controller exposes protected CRUD", GrupoObjetivoInvestimentoControllerTests.CrudReturnsProtectedTypedContracts),
    ("objective group controller maps safe errors", GrupoObjetivoInvestimentoControllerTests.MapsValidationNotFoundConflictAndFailure),
    ("investment type controller binds additive fields", InvestimentoTipoAtivoControllerTests.AdditiveTypeFieldsRoundTripOnCommands),
    ("investment organization controller binds filters", InvestimentoOrganizacaoConsultaControllerTests.BindsEveryConsolidatedFilter),
    ("investment organization controller maps query errors", InvestimentoOrganizacaoConsultaControllerTests.MapsInvalidUnavailableAndUnexpectedQueries),
    ("objective exclusion controller maps atomic command", GrupoObjetivoInvestimentoExclusaoControllerTests.ExclusionReturnsNoContentAndStableConflicts)
    ,("integrated objective supports target and lifecycle", MetasObjetivosIntegracaoDomainTests.ObjectiveSupportsTargetAndLifecycle)
    ,("monthly goal links canonical objective", MetasObjetivosIntegracaoDomainTests.MonthlyGoalCanLinkCanonicalObjective)
    ,("objective contribution is precise and allocation stable", MetasObjetivosIntegracaoDomainTests.ContributionIsPreciseAndAllocationIsStable)
    ,("goal objective link contract is owner derived", MetasObjetivosVinculoTests.ContractsPreserveOwnerDerivedCanonicalLink)
    ,("goal objective commands stay protected", MetasObjetivosVinculoControllerTests.CommandsStayProtectedAndTyped)
    ,("contribution validates precision and immutable allocation", AporteObjetivoInvestimentoTests.ValidatesPrecisionAndImmutableAllocation)
    ,("contribution operation detects replay divergence", AporteObjetivoInvestimentoServiceTests.OperationIdentityDetectsReplayAndDivergence)
    ,("contribution endpoints expose history confirmation allocation", AporteObjetivoInvestimentoControllerTests.ExposesHistoryConfirmationAndAllocationRoutes)
    ,("objective summary separates portfolio and contributions", ObjetivoInvestimentoResumoTests.SummarySeparatesPortfolioFromConfirmedContributions)
    ,("objective summary endpoint binds typed query", ObjetivoInvestimentoResumoControllerTests.SummaryEndpointUsesTypedQueryContract)
    ,("objective lifecycle preserves target", GrupoObjetivoInvestimentoCicloVidaTests.ArchiveReactivatePreservesTarget)
    ,("objective presentation supports safe image", GrupoObjetivoInvestimentoCicloVidaTests.PresentationSupportsDescriptionAndSafeInternalImage)
    ,("objective lifecycle endpoints are post commands", GrupoObjetivoInvestimentoCicloVidaControllerTests.LifecycleEndpointsArePostCommands)
    ,("activity projection is owner scoped and totals precede paging", MovimentacaoFinanceiraServiceTests.ProjectionIsOwnerScopedAndTotalsPrecedePagination)
    ,("activity query bounds are validated", MovimentacaoFinanceiraServiceTests.QueryBoundsAreValidated)
    ,("activity endpoint is protected and maps stable errors", MovimentacoesControllerTests.EndpointIsProtectedAndMapsStableErrors)
    ,("account crud uses authenticated owner and conflict detection", ContaFinanceiraServiceTests.CrudUsesAuthenticatedOwnerAndConflictDetection)
    ,("account controller is protected and typed", ContaFinanceiraControllerTests.CrudIsProtectedAndTyped)
    ,("account links and ordering stay owner scoped", ContaFinanceiraRepositoryTests.LinksAndOrderingStayOwnerScoped)
    ,("password recovery is neutral delivered and secret free", PasswordRecoveryControllerTests.RecoveryIsNeutralDeliveredAndSecretFree),
    ("Pluggy source is unique and investment targets aggregate", PluggyTransactionImportConfigurationTests.SourceIsUniqueAndInvestmentTargetsCanAggregateSources),
    ("Pluggy overview returns all typed expense candidates", PluggyControllerTests.OverviewReturnsAllTypedExpenseCandidates),
    ("Pluggy expense reconciliation uses authenticated user", PluggyControllerTests.ExpenseReconciliationUsesAuthenticatedUserAndReturnsTypedResult),
    ("Pluggy investment reconciliation uses authenticated user", PluggyControllerTests.InvestmentReconciliationUsesAuthenticatedUserAndReturnsTypedResult),
    ("Pluggy investment reconciliation maps stale review conflicts", PluggyControllerTests.InvestmentReconciliationMapsReviewConflict),
    ("Pluggy investment overview matches candidates by cents and date", PluggyInvestmentOverviewTests.SameAmountWithinOneDaySuggestsInvestmentCandidate),
    ("Pluggy investment overview exposes same-day corrections", PluggyInvestmentOverviewTests.InvestmentCandidateCanBeMatchedByDescriptionWithAmountCorrection),
    ("Pluggy investment overview scopes and returns every candidate", PluggyInvestmentOverviewTests.InvestmentOverviewScopesAndReturnsEveryCandidate),
    ("Pluggy investment asset preview uses page-numbered position endpoint", PluggyInvestmentAssetsTests.PreviewUsesInvestmentEndpointAndPagePagination),
    ("Pluggy investment asset contract preserves provider semantics", PluggyInvestmentAssetsTests.PositionContractPreservesProviderSemantics),
    ("Pluggy investment asset retry uses a controlled failure window", PluggyInvestmentAssetsTests.FailedRefreshUsesControlledRetryWindow),
    ("Pluggy investment asset retry preserves successful refresh window", PluggyInvestmentAssetsTests.SuccessfulAssetRefreshCannotBeForced),
    ("Pluggy legacy investment descriptions match without code or equal value", PluggyInvestmentAssetMatchingTests.LegacyDescriptionMatchesWithoutIdentifiersOrEqualValues),
    ("Pluggy similar investment descriptions rank by value without a stale value cutoff", PluggyInvestmentAssetMatchingTests.SimilarDescriptionsAreRankedByValueWithoutRejectingOldBalances),
    ("Pluggy investment matches reject amount only and generic product similarities", PluggyInvestmentAssetMatchingTests.SimilarValuesAndGenericWordsDoNotIdentifyInvestments),
    ("Pluggy investment matches prioritize identifiers and preserve currency", PluggyInvestmentAssetMatchingTests.IdentifiersTakePrecedenceWhenAvailableAndCurrencyIsPreserved),
    ("Pluggy confirmed investment association completes missing identity only", PluggyInvestmentAssetMatchingTests.ConfirmedAssociationCompletesOnlyMissingIdentifiers),
    ("Pluggy investment asset preview is protected and uses asset endpoint", PluggyInvestmentAssetsControllerTests.PreviewIsProtectedAndUsesAssetEndpoint),
    ("Pluggy investment asset preview JSON uses application field names", PluggyInvestmentAssetsControllerTests.PreviewJsonUsesApplicationFieldNames),
    ("Pluggy investment asset cache is persistent and owner scoped", PluggyInvestmentAssetsControllerTests.CacheAndOwnerRulesArePersisted),
    ("Pluggy investment asset reconciliation validates snapshots and writes links atomically", PluggyInvestmentReconciliationTests.AssetReconciliationUsesSnapshotAndWritesLinkAtomically),
    ("Pluggy investment asset financial precision preserves eight decimals", PluggyInvestmentReconciliationTests.InvestmentAssetQuantitiesAndQuotesKeepEightDecimals),
    ("Pluggy investment asset source link is unique and owner bound", PluggyInvestmentAssetLinkConfigurationTests.SourceIdentityIsUniqueAndOwnerBound),
    ("Pluggy investment eligibility accepts only safe finalized debits", PluggyInvestmentReconciliationTests.OnlyFinalizedBrlDebitsAreEligible),
    ("Pluggy investment eligibility requires trusted BRL value", PluggyInvestmentReconciliationTests.ForeignCurrencyWithoutTrustedBrlAmountIsNotEligible),
    ("Pluggy investment reconciliation persists position and source atomically", PluggyInvestmentReconciliationTests.ReconciliationWritesPositionAndSourceAtomically),
    ("Pluggy investment replacement preserves asset details", PluggyInvestmentReconciliationTests.ReplacementChangesOnlyBankSharedFields),
    ("Pluggy manual match explains normalized exact candidates", PluggyExpenseCandidateMatchingTests.SameDayNormalizedDescriptionAndCentsAreExplained),
    ("Pluggy manual match suggests same amount within one day", PluggyExpenseCandidateMatchingTests.SameAmountWithinOneDaySuggestsDifferentDescription),
    ("Pluggy manual match exposes same-day amount differences", PluggyExpenseCandidateMatchingTests.SameDayDescriptionCanSuggestAmountCorrection),
    ("Pluggy manual match rejects unrelated expenses", PluggyExpenseCandidateMatchingTests.UnrelatedManualExpenseIsNotSuggested),
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
