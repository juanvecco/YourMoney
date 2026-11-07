# PostgreSQL migration trial

This trial copies a SQL Server backup into a separate SQL Server database, then
uses pgloader to create and populate a PostgreSQL database. The active SQL
Server database is never a migration target.

1. Take and verify a `COPY_ONLY` SQL Server backup. Restore it under a new name.
2. Create an empty PostgreSQL database. Put the SQL Server and PostgreSQL
   credentials in a pgloader command file **outside the repository**.
3. Run pgloader with `WITH quote identifiers` and
   `ALTER SCHEMA 'dbo' RENAME TO 'public'`. Keep original table and column case:
   EF Core queries use quoted names.
4. Run [`after-pgloader.sql`](after-pgloader.sql) on the PostgreSQL database.
   It preserves .NET `DateTime` and `DateTimeOffset` mappings and converts the
   SQL Server `rowversion` to a PostgreSQL concurrency token and creates the
   persistent ASP.NET Data Protection key table.
5. Compare all source table counts, exact financial sums and all tables with
   `UsuarioId` against existing Identity users:

   ```powershell
   ./tools/postgres/verify-migration.ps1 -ManifestPath '<verified-backup-manifest.json>' -DockerContainer yourmoney-postgres-test
   ```

   The script exits with code 1 on any divergence. It requires a manifest whose
   `BackupSqlVerify` is `Passed` and whose `HostCopyHashMatches` is true. For a
   remote PostgreSQL target, install `psql`, set `PGHOST`, `PGPORT`, `PGDATABASE`,
   `PGUSER`, `PGPASSWORD`, and `PGSSLMODE=require` outside the repository, then
   run the same script without `-DockerContainer`.
6. Test reads through `AppDbContext` and `ApplicationDbContext`, then create one
   expense and one revenue inside transactions that are rolled back. Verify the
   destination counts are unchanged afterward. Never perform this step against
   the original financial database.

For local application testing, set `DATABASE_PROVIDER=PostgreSQL` and
`DATABASE_CONNECTION_STRING` to the PostgreSQL connection string. Both the app
context and the Identity context use this setting. Without it, the application
continues to use its SQL Server configuration.

The existing EF migrations are SQL Server specific and must not be applied to
PostgreSQL. The imported schema is the pilot baseline; future schema changes
need a separately reviewed PostgreSQL migration set before application. Never
run `database update` or `EnsureCreated` against this imported database.

For the business rules feature, review `add-business-rules.sql` and apply it to
the PostgreSQL pilot before deploying the API that reads `tbRegraNegocioUsuario`.
The script only creates the new choices table and indexes; it does not edit
financial transactions or existing accounts.

Pluggy credentials encrypted with the Windows DPAPI key ring cannot be decrypted
by a new Linux key ring. Re-enter them through the existing authenticated UI
after publication. The original notebook and its key ring must remain available
until that re-entry is complete.

See [DEPLOYMENT.md](DEPLOYMENT.md) for the external hosting procedure.
