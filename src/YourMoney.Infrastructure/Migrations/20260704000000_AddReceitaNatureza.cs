using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YourMoney.Infrastructure.Persistence;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260704000000_AddReceitaNatureza")]
    public partial class AddReceitaNatureza : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DespesaVinculadaId",
                table: "tbReceita",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Natureza",
                table: "tbReceita",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "RendaDisponivel");

            migrationBuilder.CreateIndex(
                name: "IX_Receita_DespesaVinculadaId",
                table: "tbReceita",
                column: "DespesaVinculadaId");

            migrationBuilder.CreateIndex(
                name: "IX_Receita_Usuario_Natureza_MesReferencia",
                table: "tbReceita",
                columns: new[] { "UsuarioId", "Natureza", "mesReferencia" });

            migrationBuilder.AddForeignKey(
                name: "FK_tbReceita_tbDespesa_DespesaVinculadaId",
                table: "tbReceita",
                column: "DespesaVinculadaId",
                principalTable: "tbDespesa",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbReceita_tbDespesa_DespesaVinculadaId",
                table: "tbReceita");

            migrationBuilder.DropIndex(
                name: "IX_Receita_DespesaVinculadaId",
                table: "tbReceita");

            migrationBuilder.DropIndex(
                name: "IX_Receita_Usuario_Natureza_MesReferencia",
                table: "tbReceita");

            migrationBuilder.DropColumn(
                name: "DespesaVinculadaId",
                table: "tbReceita");

            migrationBuilder.DropColumn(
                name: "Natureza",
                table: "tbReceita");
        }
    }
}
