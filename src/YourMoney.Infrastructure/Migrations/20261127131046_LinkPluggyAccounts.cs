using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkPluggyAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_tbContaFinanceira_Id_UsuarioId",
                table: "tbContaFinanceira",
                columns: new[] { "Id", "UsuarioId" });

            migrationBuilder.CreateTable(
                name: "tbPluggyAccountLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ContaFinanceiraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbPluggyAccountLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbPluggyAccountLink_tbContaFinanceira_ContaFinanceiraId_UsuarioId",
                        columns: x => new { x.ContaFinanceiraId, x.UsuarioId },
                        principalTable: "tbContaFinanceira",
                        principalColumns: new[] { "Id", "UsuarioId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbPluggyAccountLink_tbPluggyConnection_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "tbPluggyConnection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyAccountLink_ConnectionId",
                table: "tbPluggyAccountLink",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyAccountLink_ContaFinanceiraId_UsuarioId",
                table: "tbPluggyAccountLink",
                columns: new[] { "ContaFinanceiraId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyAccountLink_UsuarioId",
                table: "tbPluggyAccountLink",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyAccountLink_UsuarioId_ConnectionId_AccountId",
                table: "tbPluggyAccountLink",
                columns: new[] { "UsuarioId", "ConnectionId", "AccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbPluggyAccountLink");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_tbContaFinanceira_Id_UsuarioId",
                table: "tbContaFinanceira");
        }
    }
}
