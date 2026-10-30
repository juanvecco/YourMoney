using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PluggyExpenseReconciliationUniqueTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM [tbPluggyTransactionImport]
                    GROUP BY [UsuarioId], [RecordType], [RecordId]
                    HAVING COUNT_BIG(*) > 1
                )
                BEGIN
                    THROW 51000, N'Cannot enforce unique Pluggy expense links: duplicate target records exist.', 1;
                END
                """);
            migrationBuilder.DropIndex(
                name: "IX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport");

            migrationBuilder.CreateIndex(
                name: "UX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport",
                columns: new[] { "UsuarioId", "RecordType", "RecordId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport");

            migrationBuilder.CreateIndex(
                name: "IX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport",
                columns: new[] { "UsuarioId", "RecordType", "RecordId" });
        }
    }
}

