using System.Text.RegularExpressions;
using YourMoney.Tests.Fixtures;

namespace YourMoney.Tests.Infrastructure
{
    public static class FinancialOwnershipScriptTests
    {
        public static Task ScriptsContainTargetUserConstants()
        {
            var backfill = ReadScript("BackfillFinancialOwnershipToTargetUser.sql");
            var validation = ReadScript("ValidateFinancialOwnership.sql");
            var ensureSchema = ReadScript("EnsureFinancialOwnershipSchema.sql");
            var runner = File.ReadAllText(RepositoryTestPaths.InYourMoney("scripts", "Invoke-FinancialOwnershipBackfill.ps1"));

            TestAssert.True(backfill.Contains("$(TargetUserId)"), "Backfill script should receive TargetUserId from sqlcmd variables");
            TestAssert.True(validation.Contains("$(TargetUserId)"), "Validation script should receive TargetUserId from sqlcmd variables");
            TestAssert.True(ensureSchema.Contains(FinancialOwnershipTestConstants.LegacyTransitionUserId), "Schema script should initialize ownership with the transition user before backfill");
            TestAssert.True(runner.Contains(FinancialOwnershipTestConstants.TargetUserId), "Runner should default to the requested target user id");
            return Task.CompletedTask;
        }

        public static Task BackfillStopsWhenTargetUserIsMissing()
        {
            var sql = ReadScript("BackfillFinancialOwnershipToTargetUser.sql");

            TestAssert.True(sql.Contains("IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @TargetUserId)"), "Backfill should verify AspNetUsers before writing");
            TestAssert.True(sql.Contains("THROW 51002"), "Backfill should throw before writes when the target user is missing");
            TestAssert.True(sql.IndexOf("THROW 51002", StringComparison.Ordinal) < sql.IndexOf("BEGIN TRANSACTION", StringComparison.Ordinal), "Missing target user guard should run before the transaction starts");
            return Task.CompletedTask;
        }

        public static Task BackfillChecksUsuarioIdColumnsBeforeWriting()
        {
            var sql = ReadScript("BackfillFinancialOwnershipToTargetUser.sql");

            TestAssert.True(sql.Contains("COL_LENGTH(N'dbo.' + TableName, N'UsuarioId') IS NULL"), "Backfill should detect missing UsuarioId columns");
            TestAssert.True(sql.Contains("THROW 51003"), "Backfill should stop when ownership schema is incomplete");
            TestAssert.True(sql.IndexOf("THROW 51003", StringComparison.Ordinal) < sql.IndexOf("BEGIN TRANSACTION", StringComparison.Ordinal), "Schema guard should run before the transaction starts");
            return Task.CompletedTask;
        }

        public static Task BackfillUpdatesOnlyOwnershipColumns()
        {
            var sql = ReadScript("BackfillFinancialOwnershipToTargetUser.sql");
            TestAssert.True(sql.Contains("UPDATE dbo.' + QUOTENAME(@TableName)"), "Backfill should update only scoped tables from the explicit table list");
            TestAssert.True(sql.Contains("SET UsuarioId = @TargetUserId"), "Backfill should assign only UsuarioId");
            TestAssert.True(sql.Contains("WHERE ISNULL(UsuarioId, N'''') <> @TargetUserId"), "Backfill should only update rows not already owned by the target user");

            var forbiddenAssignments = new[]
            {
                "SET Valor",
                "SET Data",
                "SET Descricao",
                "SET ParcelamentoId",
                "SET NumeroParcela",
                "SET TotalParcelas",
                "SET ValorTotalParcelamento",
                "SET IdContaFinanceira",
                "SET IdCategoria",
                "SET ValorAtual",
                "SET ValorObjetivo"
            };

            foreach (var forbidden in forbiddenAssignments)
                TestAssert.True(!sql.Contains(forbidden, StringComparison.OrdinalIgnoreCase), $"Backfill must not assign financial field: {forbidden}");

            return Task.CompletedTask;
        }

        public static Task BackfillIsIdempotent()
        {
            var sql = ReadScript("BackfillFinancialOwnershipToTargetUser.sql");
            foreach (var table in FinancialOwnershipTestConstants.ScopedTables)
                TestAssert.True(sql.Contains($"(N'{table}')"), $"Backfill should include {table} in the scoped table list");

            TestAssert.True(sql.Contains("WHERE ISNULL(UsuarioId, N'''') <> @TargetUserId"), "Backfill should update only rows not already owned by the target user");
            return Task.CompletedTask;
        }

        public static Task BackfillUsesTransactionAndDryRunRollback()
        {
            var sql = ReadScript("BackfillFinancialOwnershipToTargetUser.sql");

            TestAssert.True(sql.Contains("BEGIN TRANSACTION"), "Backfill should run inside a transaction");
            TestAssert.True(sql.Contains("ROLLBACK TRANSACTION"), "Backfill should be able to roll back");
            TestAssert.True(sql.Contains("IF @DryRun = 1"), "Backfill should support dry-run mode");
            TestAssert.True(sql.Contains("COMMIT TRANSACTION"), "Backfill should commit only after validation");
            return Task.CompletedTask;
        }

        public static Task ValidationDetectsInvalidOwners()
        {
            var sql = ReadScript("ValidateFinancialOwnership.sql");

            TestAssert.True(sql.Contains("RowsWithMissingIdentityUser"), "Validation should report rows whose UsuarioId is absent from AspNetUsers");
            TestAssert.True(sql.Contains("LEFT JOIN dbo.AspNetUsers u ON u.Id = f.UsuarioId"), "Validation should compare scoped rows against AspNetUsers");
            TestAssert.True(sql.Contains(FinancialOwnershipTestConstants.LegacyTransitionUserId), "Validation should report transition-owner rows");
            return Task.CompletedTask;
        }

        public static Task ValidationDetectsRelationshipMismatches()
        {
            var sql = ReadScript("ValidateFinancialOwnership.sql");

            TestAssert.True(sql.Contains("tbDespesa->tbCategoria"), "Validation should check despesa/categoria owner consistency");
            TestAssert.True(sql.Contains("tbDespesa->tbContaFinanceira"), "Validation should check despesa/conta owner consistency");
            TestAssert.True(sql.Contains("tbMeta->tbCategoria"), "Validation should check meta/categoria owner consistency");
            TestAssert.True(sql.Contains("tbCategoria->tbCategoriaPai"), "Validation should check parent category owner consistency");
            return Task.CompletedTask;
        }

        public static Task ValidationReportsFinancialTotalsAndDates()
        {
            var sql = ReadScript("ValidateFinancialOwnership.sql");

            TestAssert.True(sql.Contains("FinancialTotals"), "Validation should report financial totals");
            TestAssert.True(sql.Contains("SUM(CAST(Valor AS decimal(38,2)))"), "Validation should total despesas/receitas values");
            TestAssert.True(sql.Contains("SUM(CAST(ValorAtual AS decimal(38,2)))"), "Validation should total investment current values");
            TestAssert.True(sql.Contains("MIN(Data)") && sql.Contains("MAX(Data)"), "Validation should report date ranges");
            return Task.CompletedTask;
        }

        public static Task RunnerSupportsValidateAndExecuteModes()
        {
            var runner = File.ReadAllText(RepositoryTestPaths.InYourMoney("scripts", "Invoke-FinancialOwnershipBackfill.ps1"));

            TestAssert.True(runner.Contains("[ValidateSet('EnsureSchema', 'Validate', 'Execute')]"), "Runner should expose schema, validate and execute modes");
            TestAssert.True(runner.Contains("sqlcmd"), "Runner should execute SQL through sqlcmd");
            TestAssert.True(runner.Contains("DATABASE_CONNECTION_STRING"), "Runner should support DATABASE_CONNECTION_STRING");
            TestAssert.True(runner.Contains("DefaultConnection"), "Runner should fall back to the API DefaultConnection");
            return Task.CompletedTask;
        }

        private static string ReadScript(string name)
            => File.ReadAllText(RepositoryTestPaths.InYourMoney("src", "YourMoney.Infrastructure", "Scripts", name));
    }
}
