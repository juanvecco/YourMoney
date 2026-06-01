SET NOCOUNT ON;

DECLARE @TargetUserId nvarchar(450) = N'$(TargetUserId)';
DECLARE @LegacyUserId nvarchar(450) = N'legacy-transition-user';

IF @TargetUserId IS NULL OR LTRIM(RTRIM(@TargetUserId)) = N''
BEGIN
    THROW 51000, 'TargetUserId is required.', 1;
END;

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
    THROW 51001, 'AspNetUsers table was not found in the current database.', 1;
END;

SELECT
    'TargetUser' AS CheckName,
    CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @TargetUserId) THEN 1 ELSE 0 END AS int) AS Passed,
    @TargetUserId AS TargetUserId;

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @TargetUserId)
BEGIN
    THROW 51002, 'Target user was not found in AspNetUsers.', 1;
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

DECLARE @TableName sysname;
DECLARE @Sql nvarchar(max);

DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName FROM @ScopedTables ORDER BY TableName;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(@TableName), N'U') IS NULL
    BEGIN
        SELECT 'MissingTable' AS CheckName, @TableName AS TableName, 1 AS CountValue;
    END
    ELSE IF COL_LENGTH(N'dbo.' + @TableName, N'UsuarioId') IS NULL
    BEGIN
        SELECT 'MissingUsuarioIdColumn' AS CheckName, @TableName AS TableName, 1 AS CountValue;
    END
    ELSE
    BEGIN
        SET @Sql = N'
SELECT ''RowsByOwner'' AS CheckName, N''' + @TableName + N''' AS TableName, UsuarioId, COUNT_BIG(*) AS CountValue
FROM dbo.' + QUOTENAME(@TableName) + N'
GROUP BY UsuarioId
ORDER BY UsuarioId;

SELECT ''RowsOwnedByTarget'' AS CheckName, N''' + @TableName + N''' AS TableName, COUNT_BIG(*) AS CountValue
FROM dbo.' + QUOTENAME(@TableName) + N'
WHERE UsuarioId = @TargetUserId;

SELECT ''RowsOwnedByLegacyTransitionUser'' AS CheckName, N''' + @TableName + N''' AS TableName, COUNT_BIG(*) AS CountValue
FROM dbo.' + QUOTENAME(@TableName) + N'
WHERE UsuarioId = @LegacyUserId;

SELECT ''RowsWithMissingIdentityUser'' AS CheckName, N''' + @TableName + N''' AS TableName, COUNT_BIG(*) AS CountValue
FROM dbo.' + QUOTENAME(@TableName) + N' f
LEFT JOIN dbo.AspNetUsers u ON u.Id = f.UsuarioId
WHERE u.Id IS NULL;';

        EXEC sp_executesql
            @Sql,
            N'@TargetUserId nvarchar(450), @LegacyUserId nvarchar(450)',
            @TargetUserId = @TargetUserId,
            @LegacyUserId = @LegacyUserId;
    END;

    FETCH NEXT FROM table_cursor INTO @TableName;
END;

CLOSE table_cursor;
DEALLOCATE table_cursor;

IF OBJECT_ID(N'dbo.tbDespesa', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbDespesa', N'UsuarioId') IS NOT NULL
   AND OBJECT_ID(N'dbo.tbCategoria', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbCategoria', N'UsuarioId') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
    SELECT ''RelationshipOwnerMismatch'' AS CheckName, ''tbDespesa->tbCategoria'' AS RelationshipName, COUNT_BIG(*) AS CountValue
    FROM dbo.tbDespesa d
    INNER JOIN dbo.tbCategoria c ON c.Id = d.IdCategoria
    WHERE d.UsuarioId <> c.UsuarioId;';
END;

IF OBJECT_ID(N'dbo.tbDespesa', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbDespesa', N'UsuarioId') IS NOT NULL
   AND OBJECT_ID(N'dbo.tbContaFinanceira', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbContaFinanceira', N'UsuarioId') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
    SELECT ''RelationshipOwnerMismatch'' AS CheckName, ''tbDespesa->tbContaFinanceira'' AS RelationshipName, COUNT_BIG(*) AS CountValue
    FROM dbo.tbDespesa d
    INNER JOIN dbo.tbContaFinanceira c ON c.Id = d.IdContaFinanceira
    WHERE d.UsuarioId <> c.UsuarioId;';
END;

IF OBJECT_ID(N'dbo.tbMeta', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbMeta', N'UsuarioId') IS NOT NULL
   AND OBJECT_ID(N'dbo.tbCategoria', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbCategoria', N'UsuarioId') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
    SELECT ''RelationshipOwnerMismatch'' AS CheckName, ''tbMeta->tbCategoria'' AS RelationshipName, COUNT_BIG(*) AS CountValue
    FROM dbo.tbMeta m
    INNER JOIN dbo.tbCategoria c ON c.Id = m.CategoriaId
    WHERE m.CategoriaId IS NOT NULL
      AND m.UsuarioId <> c.UsuarioId;';
END;

IF OBJECT_ID(N'dbo.tbCategoria', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbCategoria', N'UsuarioId') IS NOT NULL
BEGIN
    EXEC sp_executesql N'
    SELECT ''RelationshipOwnerMismatch'' AS CheckName, ''tbCategoria->tbCategoriaPai'' AS RelationshipName, COUNT_BIG(*) AS CountValue
    FROM dbo.tbCategoria c
    INNER JOIN dbo.tbCategoria p ON p.Id = c.CategoriaPaiId
    WHERE c.CategoriaPaiId IS NOT NULL
      AND c.UsuarioId <> p.UsuarioId;';
END;

IF OBJECT_ID(N'dbo.tbDespesa', N'U') IS NOT NULL
BEGIN
    SELECT 'FinancialTotals' AS CheckName, 'tbDespesa' AS TableName,
           COUNT_BIG(*) AS RowsValue,
           COALESCE(SUM(CAST(Valor AS decimal(38,2))), 0) AS TotalValue,
           MIN(Data) AS MinDate,
           MAX(Data) AS MaxDate
    FROM dbo.tbDespesa;
END;

IF OBJECT_ID(N'dbo.tbReceita', N'U') IS NOT NULL
BEGIN
    SELECT 'FinancialTotals' AS CheckName, 'tbReceita' AS TableName,
           COUNT_BIG(*) AS RowsValue,
           COALESCE(SUM(CAST(Valor AS decimal(38,2))), 0) AS TotalValue,
           MIN(Data) AS MinDate,
           MAX(Data) AS MaxDate
    FROM dbo.tbReceita;
END;

IF OBJECT_ID(N'dbo.tbInvestimento', N'U') IS NOT NULL
BEGIN
    SELECT 'FinancialTotals' AS CheckName, 'tbInvestimento' AS TableName,
           COUNT_BIG(*) AS RowsValue,
           COALESCE(SUM(CAST(ValorAtual AS decimal(38,2))), 0) AS TotalValue,
           MIN(DataInvestimento) AS MinDate,
           MAX(DataInvestimento) AS MaxDate
    FROM dbo.tbInvestimento;
END;

IF OBJECT_ID(N'dbo.tbMeta', N'U') IS NOT NULL
BEGIN
    SELECT 'FinancialTotals' AS CheckName, 'tbMeta' AS TableName,
           COUNT_BIG(*) AS RowsValue,
           COALESCE(SUM(CAST(ValorObjetivo AS decimal(38,2))), 0) AS TotalObjetivo,
           COALESCE(SUM(CAST(ValorAtual AS decimal(38,2))), 0) AS TotalAtual,
           MIN(DataInicio) AS MinDate,
           MAX(DataObjetivo) AS MaxDate
    FROM dbo.tbMeta;
END;

SELECT 'ValidationComplete' AS CheckName, @TargetUserId AS TargetUserId, SYSDATETIMEOFFSET() AS CompletedAt;
