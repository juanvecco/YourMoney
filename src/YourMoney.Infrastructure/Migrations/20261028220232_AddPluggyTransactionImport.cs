using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPluggyTransactionImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbPluggyTransactionImport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisteredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbPluggyTransactionImport", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PluggyTransactionImport_Usuario_Record",
                table: "tbPluggyTransactionImport",
                columns: new[] { "UsuarioId", "RecordType", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyTransactionImport_UsuarioId",
                table: "tbPluggyTransactionImport",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_PluggyTransactionImport_Usuario_Transaction",
                table: "tbPluggyTransactionImport",
                columns: new[] { "UsuarioId", "TransactionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbPluggyTransactionImport");
        }
    }
}
