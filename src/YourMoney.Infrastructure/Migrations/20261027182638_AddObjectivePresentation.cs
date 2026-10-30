using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddObjectivePresentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "tbInvestimentoGrupoObjetivo",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "tbInvestimentoGrupoObjetivo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "tbInvestimentoGrupoObjetivo");
        }
    }
}
