SET NOCOUNT ON;

DECLARE @LegacyUserId nvarchar(450) = N'legacy-transition-user';

IF OBJECT_ID(N'dbo.tbCategoria', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.tbCategoria', N'UsuarioId') IS NULL
BEGIN
    ALTER TABLE dbo.tbCategoria ADD UsuarioId nvarchar(450) NOT NULL CONSTRAINT DF_tbCategoria_UsuarioId DEFAULT(N'legacy-transition-user') WITH VALUES;
    SELECT 'ColumnAdded' AS CheckName, 'tbCategoria' AS TableName, @LegacyUserId AS InitialOwner;
END;

IF OBJECT_ID(N'dbo.tbContaFinanceira', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.tbContaFinanceira', N'UsuarioId') IS NULL
BEGIN
    ALTER TABLE dbo.tbContaFinanceira ADD UsuarioId nvarchar(450) NOT NULL CONSTRAINT DF_tbContaFinanceira_UsuarioId DEFAULT(N'legacy-transition-user') WITH VALUES;
    SELECT 'ColumnAdded' AS CheckName, 'tbContaFinanceira' AS TableName, @LegacyUserId AS InitialOwner;
END;

IF OBJECT_ID(N'dbo.tbDespesa', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.tbDespesa', N'UsuarioId') IS NULL
BEGIN
    ALTER TABLE dbo.tbDespesa ADD UsuarioId nvarchar(450) NOT NULL CONSTRAINT DF_tbDespesa_UsuarioId DEFAULT(N'legacy-transition-user') WITH VALUES;
    SELECT 'ColumnAdded' AS CheckName, 'tbDespesa' AS TableName, @LegacyUserId AS InitialOwner;
END;

IF OBJECT_ID(N'dbo.tbInvestimento', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.tbInvestimento', N'UsuarioId') IS NULL
BEGIN
    ALTER TABLE dbo.tbInvestimento ADD UsuarioId nvarchar(450) NOT NULL CONSTRAINT DF_tbInvestimento_UsuarioId DEFAULT(N'legacy-transition-user') WITH VALUES;
    SELECT 'ColumnAdded' AS CheckName, 'tbInvestimento' AS TableName, @LegacyUserId AS InitialOwner;
END;

IF OBJECT_ID(N'dbo.tbMeta', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.tbMeta', N'UsuarioId') IS NULL
BEGIN
    ALTER TABLE dbo.tbMeta ADD UsuarioId nvarchar(450) NOT NULL CONSTRAINT DF_tbMeta_UsuarioId DEFAULT(N'legacy-transition-user') WITH VALUES;
    SELECT 'ColumnAdded' AS CheckName, 'tbMeta' AS TableName, @LegacyUserId AS InitialOwner;
END;

IF OBJECT_ID(N'dbo.tbReceita', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.tbReceita', N'UsuarioId') IS NULL
BEGIN
    ALTER TABLE dbo.tbReceita ADD UsuarioId nvarchar(450) NOT NULL CONSTRAINT DF_tbReceita_UsuarioId DEFAULT(N'legacy-transition-user') WITH VALUES;
    SELECT 'ColumnAdded' AS CheckName, 'tbReceita' AS TableName, @LegacyUserId AS InitialOwner;
END;

IF OBJECT_ID(N'dbo.tbCategoria', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbCategoria', N'UsuarioId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_tbCategoria_UsuarioId' AND object_id = OBJECT_ID(N'dbo.tbCategoria'))
BEGIN
    CREATE INDEX IX_tbCategoria_UsuarioId ON dbo.tbCategoria (UsuarioId);
    SELECT 'IndexCreated' AS CheckName, 'tbCategoria' AS TableName, 'IX_tbCategoria_UsuarioId' AS IndexName;
END;

IF OBJECT_ID(N'dbo.tbContaFinanceira', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbContaFinanceira', N'UsuarioId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_tbContaFinanceira_UsuarioId' AND object_id = OBJECT_ID(N'dbo.tbContaFinanceira'))
BEGIN
    CREATE INDEX IX_tbContaFinanceira_UsuarioId ON dbo.tbContaFinanceira (UsuarioId);
    SELECT 'IndexCreated' AS CheckName, 'tbContaFinanceira' AS TableName, 'IX_tbContaFinanceira_UsuarioId' AS IndexName;
END;

IF OBJECT_ID(N'dbo.tbDespesa', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbDespesa', N'UsuarioId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_tbDespesa_UsuarioId' AND object_id = OBJECT_ID(N'dbo.tbDespesa'))
BEGIN
    CREATE INDEX IX_tbDespesa_UsuarioId ON dbo.tbDespesa (UsuarioId);
    SELECT 'IndexCreated' AS CheckName, 'tbDespesa' AS TableName, 'IX_tbDespesa_UsuarioId' AS IndexName;
END;

IF OBJECT_ID(N'dbo.tbInvestimento', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbInvestimento', N'UsuarioId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_tbInvestimento_UsuarioId' AND object_id = OBJECT_ID(N'dbo.tbInvestimento'))
BEGIN
    CREATE INDEX IX_tbInvestimento_UsuarioId ON dbo.tbInvestimento (UsuarioId);
    SELECT 'IndexCreated' AS CheckName, 'tbInvestimento' AS TableName, 'IX_tbInvestimento_UsuarioId' AS IndexName;
END;

IF OBJECT_ID(N'dbo.tbMeta', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbMeta', N'UsuarioId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_tbMeta_UsuarioId' AND object_id = OBJECT_ID(N'dbo.tbMeta'))
BEGIN
    CREATE INDEX IX_tbMeta_UsuarioId ON dbo.tbMeta (UsuarioId);
    SELECT 'IndexCreated' AS CheckName, 'tbMeta' AS TableName, 'IX_tbMeta_UsuarioId' AS IndexName;
END;

IF OBJECT_ID(N'dbo.tbReceita', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbReceita', N'UsuarioId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_tbReceita_UsuarioId' AND object_id = OBJECT_ID(N'dbo.tbReceita'))
BEGIN
    CREATE INDEX IX_tbReceita_UsuarioId ON dbo.tbReceita (UsuarioId);
    SELECT 'IndexCreated' AS CheckName, 'tbReceita' AS TableName, 'IX_tbReceita_UsuarioId' AS IndexName;
END;

SELECT 'SchemaEnsureComplete' AS CheckName, SYSDATETIMEOFFSET() AS CompletedAt;
