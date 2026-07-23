BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    ALTER TABLE [tbInvestimento] ADD [OperacaoId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    ALTER TABLE [tbInvestimento] ADD [ReceitaRecorrenteId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    CREATE INDEX [IX_Investimento_UsuarioId_DataInvestimento] ON [tbInvestimento] ([UsuarioId], [DataInvestimento]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    CREATE INDEX [IX_Investimento_UsuarioId_ReceitaRecorrenteId] ON [tbInvestimento] ([UsuarioId], [ReceitaRecorrenteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    CREATE INDEX [IX_tbInvestimento_ReceitaRecorrenteId] ON [tbInvestimento] ([ReceitaRecorrenteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Investimento_UsuarioId_OperacaoId] ON [tbInvestimento] ([UsuarioId], [OperacaoId]) WHERE [OperacaoId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    ALTER TABLE [tbInvestimento] ADD CONSTRAINT [FK_tbInvestimento_tbReceitaRecorrente_ReceitaRecorrenteId] FOREIGN KEY ([ReceitaRecorrenteId]) REFERENCES [tbReceitaRecorrente] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260722210024_AddInvestimentosReservaSalarial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260722210024_AddInvestimentosReservaSalarial', N'9.0.0');
END;

COMMIT;
GO

