using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PluggyInvestmentReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport");

            migrationBuilder.CreateIndex(
                name: "UX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport",
                columns: new[] { "UsuarioId", "RecordType", "RecordId" },
                unique: true,
                filter: "[RecordType] <> 'investment'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM [tbPluggyTransactionImport]
                    WHERE [RecordType] = 'investment'
                    GROUP BY [UsuarioId], [RecordType], [RecordId]
                    HAVING COUNT_BIG(*) > 1
                )
                BEGIN
                    THROW 51000, N'Cannot restore unique Pluggy targets while an investment has multiple source transactions.', 1;
                END
                """);

            migrationBuilder.DropIndex(
                name: "UX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport");

            migrationBuilder.CreateIndex(
                name: "UX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport",
                columns: new[] { "UsuarioId", "RecordType", "RecordId" },
                unique: true);
        }
    }
}
