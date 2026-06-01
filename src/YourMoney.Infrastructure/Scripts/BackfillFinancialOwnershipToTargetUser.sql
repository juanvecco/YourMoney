SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TargetUserId nvarchar(450) = N'$(TargetUserId)';
DECLARE @DryRun bit = CASE WHEN N'$(DryRun)' IN (N'1', N'true', N'True', N'TRUE') THEN 1 ELSE 0 END;

IF @TargetUserId IS NULL OR LTRIM(RTRIM(@TargetUserId)) = N''
BEGIN
    THROW 51000, 'TargetUserId is required.', 1;
END;

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
    THROW 51001, 'AspNetUsers table was not found in the current database.', 1;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @TargetUserId)
BEGIN
    THROW 51002, 'Target user was not found in AspNetUsers. No financial rows were changed.', 1;
END;

DECLARE @ScopedTables TABLE (TableName sysname NOT NULL PRIMARY KEY);
INSERT INTO @ScopedTables (TableName)
VALUES
    (N'tbCategoria'),
    (N'tbContaFinanceira'),
    (N'tbDespesa'),
    (N'tbInvestimento'),
    (N'tbMeta'),
    (N'tbReceita');

DECLARE @MissingSchema TABLE (TableName sysname NOT NULL, Reason nvarchar(200) NOT NULL);

INSERT INTO @MissingSchema (TableName, Reason)
SELECT TableName, N'Missing table'
FROM @ScopedTables
WHERE OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(TableName), N'U') IS NULL;

INSERT INTO @MissingSchema (TableName, Reason)
SELECT TableName, N'Missing UsuarioId column'
FROM @ScopedTables
WHERE OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(TableName), N'U') IS NOT NULL
  AND COL_LENGTH(N'dbo.' + TableName, N'UsuarioId') IS NULL;

IF EXISTS (SELECT 1 FROM @MissingSchema)
BEGIN
    SELECT 'SchemaValidationFailed' AS CheckName, TableName, Reason FROM @MissingSchema ORDER BY TableName;
    THROW 51003, 'Financial ownership schema is incomplete. No financial rows were changed.', 1;
END;

SELECT 'BackfillMode' AS CheckName, @TargetUserId AS TargetUserId, @DryRun AS DryRun;

DECLARE @TableName sysname;
DECLARE @Sql nvarchar(max);
DECLARE @RowsChanged bigint;
DECLARE @InvalidRows bit = 0;
DECLARE @ChangedRows TABLE (TableName sysname NOT NULL, RowsChanged bigint NOT NULL);

DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName FROM @ScopedTables ORDER BY TableName;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @Sql = N'
SELECT ''PreRowsByOwner'' AS CheckName, N''' + @TableName + N''' AS TableName, UsuarioId, COUNT_BIG(*) AS CountValue
FROM dbo.' + QUOTENAME(@TableName) + N'
GROUP BY UsuarioId
ORDER BY UsuarioId;';

    EXEC sp_executesql @Sql;
    FETCH NEXT FROM table_cursor INTO @TableName;
END;

CLOSE table_cursor;

SELECT 'PreFinancialTotals' AS CheckName, 'tbDespesa' AS TableName, COUNT_BIG(*) AS RowsValue,
       COALESCE(SUM(CAST(Valor AS decimal(38,2))), 0) AS TotalValue,
       MIN(Data) AS MinDate, MAX(Data) AS MaxDate
FROM dbo.tbDespesa
UNION ALL
SELECT 'PreFinancialTotals', 'tbReceita', COUNT_BIG(*),
       COALESCE(SUM(CAST(Valor AS decimal(38,2))), 0),
       MIN(Data), MAX(Data)
FROM dbo.tbReceita
UNION ALL
SELECT 'PreFinancialTotals', 'tbInvestimento', COUNT_BIG(*),
       COALESCE(SUM(CAST(ValorAtual AS decimal(38,2))), 0),
       MIN(DataInvestimento), MAX(DataInvestimento)
FROM dbo.tbInvestimento;

BEGIN TRY
    BEGIN TRANSACTION;

    OPEN table_cursor;
    FETCH NEXT FROM table_cursor INTO @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @RowsChanged = 0;
        SET @Sql = N'
UPDATE dbo.' + QUOTENAME(@TableName) + N'
SET UsuarioId = @TargetUserId
WHERE ISNULL(UsuarioId, N'''') <> @TargetUserId;
SET @RowsChanged = @@ROWCOUNT;';

        EXEC sp_executesql
            @Sql,
            N'@TargetUserId nvarchar(450), @RowsChanged bigint OUTPUT',
            @TargetUserId = @TargetUserId,
            @RowsChanged = @RowsChanged OUTPUT;

        INSERT INTO @ChangedRows VALUES (@TableName, @RowsChanged);

        SET @Sql = N'
IF EXISTS (SELECT 1 FROM dbo.' + QUOTENAME(@TableName) + N' WHERE UsuarioId <> @TargetUserId)
BEGIN
    SET @InvalidRows = 1;
END;';

        EXEC sp_executesql
            @Sql,
            N'@TargetUserId nvarchar(450), @InvalidRows bit OUTPUT',
            @TargetUserId = @TargetUserId,
            @InvalidRows = @InvalidRows OUTPUT;

        FETCH NEXT FROM table_cursor INTO @TableName;
    END;

    CLOSE table_cursor;

    IF @InvalidRows = 1
    BEGIN
        THROW 51004, 'Post-update validation found rows outside the target user.', 1;
    END;

    SELECT 'RowsChanged' AS CheckName, TableName, RowsChanged
    FROM @ChangedRows
    ORDER BY TableName;

    OPEN table_cursor;
    FETCH NEXT FROM table_cursor INTO @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Sql = N'
SELECT ''PostRowsByOwner'' AS CheckName, N''' + @TableName + N''' AS TableName, UsuarioId, COUNT_BIG(*) AS CountValue
FROM dbo.' + QUOTENAME(@TableName) + N'
GROUP BY UsuarioId
ORDER BY UsuarioId;';

        EXEC sp_executesql @Sql;
        FETCH NEXT FROM table_cursor INTO @TableName;
    END;

    CLOSE table_cursor;

    SELECT 'PostFinancialTotals' AS CheckName, 'tbDespesa' AS TableName, COUNT_BIG(*) AS RowsValue,
           COALESCE(SUM(CAST(Valor AS decimal(38,2))), 0) AS TotalValue,
           MIN(Data) AS MinDate, MAX(Data) AS MaxDate
    FROM dbo.tbDespesa
    UNION ALL
    SELECT 'PostFinancialTotals', 'tbReceita', COUNT_BIG(*),
           COALESCE(SUM(CAST(Valor AS decimal(38,2))), 0),
           MIN(Data), MAX(Data)
    FROM dbo.tbReceita
    UNION ALL
    SELECT 'PostFinancialTotals', 'tbInvestimento', COUNT_BIG(*),
           COALESCE(SUM(CAST(ValorAtual AS decimal(38,2))), 0),
           MIN(DataInvestimento), MAX(DataInvestimento)
    FROM dbo.tbInvestimento;

    IF @DryRun = 1
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'DryRunRollback' AS CheckName, 'Rolled back by request; no financial rows were changed.' AS Message;
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;
        SELECT 'Committed' AS CheckName, 'Financial ownership backfill committed.' AS Message;
    END;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'table_cursor') >= -1
        CLOSE table_cursor;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT 'BackfillFailed' AS CheckName,
           ERROR_NUMBER() AS ErrorNumber,
           ERROR_MESSAGE() AS ErrorMessage;
    THROW;
END CATCH;

DEALLOCATE table_cursor;

SELECT 'BackfillComplete' AS CheckName, @TargetUserId AS TargetUserId, @DryRun AS DryRun, SYSDATETIMEOFFSET() AS CompletedAt;
