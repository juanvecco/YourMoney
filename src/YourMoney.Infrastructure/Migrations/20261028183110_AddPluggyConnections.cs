using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPluggyConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbPluggyConnection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluggyItemId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ConnectorId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsSandbox = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConnectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbPluggyConnection", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyConnection_PluggyItemId",
                table: "tbPluggyConnection",
                column: "PluggyItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyConnection_UsuarioId",
                table: "tbPluggyConnection",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbPluggyConnection");
        }
    }
}
