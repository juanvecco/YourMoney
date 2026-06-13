using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YourMoney.Infrastructure.Persistence;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260613000000_AddInvestimentoMesReferencia")]
    public partial class AddInvestimentoMesReferencia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "mesReferencia",
                table: "tbInvestimento",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Investimento_MesReferencia",
                table: "tbInvestimento",
                column: "mesReferencia");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Investimento_MesReferencia",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "mesReferencia",
                table: "tbInvestimento");
        }
    }
}
