using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OpenFinanceInvestmentAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasInvestmentProduct",
                table: "tbPluggyConnection",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InvestmentAssetsLastAttemptAt",
                table: "tbPluggyConnection",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InvestmentAssetsLastSuccessAt",
                table: "tbPluggyConnection",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestmentAssetsSyncMessage",
                table: "tbPluggyConnection",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestmentAssetsSyncStatus",
                table: "tbPluggyConnection",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "never");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantidade",
                table: "tbInvestimento",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoMedio",
                table: "tbInvestimento",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "CodigoAtivo",
                table: "tbInvestimento",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "tbInvestimento",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoUnitarioAtual",
                table: "tbInvestimento",
                type: "decimal(18,8)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbPluggyInvestmentAssetLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluggyItemId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ExternalInvestmentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    InvestimentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    RegisteredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbPluggyInvestmentAssetLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbPluggyInvestmentAssetLink_tbInvestimento_InvestimentoId_UsuarioId",
                        columns: x => new { x.InvestimentoId, x.UsuarioId },
                        principalTable: "tbInvestimento",
                        principalColumns: new[] { "Id", "UsuarioId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbPluggyInvestmentAssetLink_tbPluggyConnection_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "tbPluggyConnection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbPluggyInvestmentAssetSnapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluggyItemId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ExternalInvestmentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subtype = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    CurrentUnitQuote = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PositionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PurchaseDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Issuer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssuerCnpj = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbPluggyInvestmentAssetSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbPluggyInvestmentAssetSnapshot_tbPluggyConnection_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "tbPluggyConnection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PluggyInvestmentAssetLink_Investimento",
                table: "tbPluggyInvestmentAssetLink",
                columns: new[] { "UsuarioId", "InvestimentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyInvestmentAssetLink_ConnectionId",
                table: "tbPluggyInvestmentAssetLink",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyInvestmentAssetLink_InvestimentoId_UsuarioId",
                table: "tbPluggyInvestmentAssetLink",
                columns: new[] { "InvestimentoId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyInvestmentAssetLink_UsuarioId",
                table: "tbPluggyInvestmentAssetLink",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_PluggyInvestmentAssetLink_Source",
                table: "tbPluggyInvestmentAssetLink",
                columns: new[] { "UsuarioId", "PluggyItemId", "ExternalInvestmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluggyInvestmentAssetSnapshot_Connection",
                table: "tbPluggyInvestmentAssetSnapshot",
                columns: new[] { "UsuarioId", "ConnectionId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyInvestmentAssetSnapshot_ConnectionId",
                table: "tbPluggyInvestmentAssetSnapshot",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyInvestmentAssetSnapshot_UsuarioId",
                table: "tbPluggyInvestmentAssetSnapshot",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_PluggyInvestmentAssetSnapshot_Source",
                table: "tbPluggyInvestmentAssetSnapshot",
                columns: new[] { "UsuarioId", "PluggyItemId", "ExternalInvestmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbPluggyInvestmentAssetLink");

            migrationBuilder.DropTable(
                name: "tbPluggyInvestmentAssetSnapshot");

            migrationBuilder.DropColumn(
                name: "HasInvestmentProduct",
                table: "tbPluggyConnection");

            migrationBuilder.DropColumn(
                name: "InvestmentAssetsLastAttemptAt",
                table: "tbPluggyConnection");

            migrationBuilder.DropColumn(
                name: "InvestmentAssetsLastSuccessAt",
                table: "tbPluggyConnection");

            migrationBuilder.DropColumn(
                name: "InvestmentAssetsSyncMessage",
                table: "tbPluggyConnection");

            migrationBuilder.DropColumn(
                name: "InvestmentAssetsSyncStatus",
                table: "tbPluggyConnection");

            migrationBuilder.DropColumn(
                name: "CodigoAtivo",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "PrecoUnitarioAtual",
                table: "tbInvestimento");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantidade",
                table: "tbInvestimento",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecoMedio",
                table: "tbInvestimento",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");
        }
    }
}
