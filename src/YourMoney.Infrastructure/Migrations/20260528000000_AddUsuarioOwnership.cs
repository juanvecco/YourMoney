using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YourMoney.Infrastructure.Persistence;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260528000000_AddUsuarioOwnership")]
    public partial class AddUsuarioOwnership : Migration
    {
        private const string LegacyUser = "legacy-transition-user";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddUsuarioId(migrationBuilder, "tbCategoria", "IX_tbCategoria_UsuarioId");
            AddUsuarioId(migrationBuilder, "tbContaFinanceira", "IX_tbContaFinanceira_UsuarioId");
            AddUsuarioId(migrationBuilder, "tbDespesa", "IX_tbDespesa_UsuarioId");
            AddUsuarioId(migrationBuilder, "tbInvestimento", "IX_tbInvestimento_UsuarioId");
            AddUsuarioId(migrationBuilder, "tbMeta", "IX_tbMeta_UsuarioId");
            AddUsuarioId(migrationBuilder, "tbReceita", "IX_tbReceita_UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropUsuarioId(migrationBuilder, "tbReceita", "IX_tbReceita_UsuarioId");
            DropUsuarioId(migrationBuilder, "tbMeta", "IX_tbMeta_UsuarioId");
            DropUsuarioId(migrationBuilder, "tbInvestimento", "IX_tbInvestimento_UsuarioId");
            DropUsuarioId(migrationBuilder, "tbDespesa", "IX_tbDespesa_UsuarioId");
            DropUsuarioId(migrationBuilder, "tbContaFinanceira", "IX_tbContaFinanceira_UsuarioId");
            DropUsuarioId(migrationBuilder, "tbCategoria", "IX_tbCategoria_UsuarioId");
        }

        private static void AddUsuarioId(MigrationBuilder migrationBuilder, string table, string indexName)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: table,
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: LegacyUser);

            migrationBuilder.CreateIndex(
                name: indexName,
                table: table,
                column: "UsuarioId");
        }

        private static void DropUsuarioId(MigrationBuilder migrationBuilder, string table, string indexName)
        {
            migrationBuilder.DropIndex(
                name: indexName,
                table: table);

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: table);
        }
    }
}
