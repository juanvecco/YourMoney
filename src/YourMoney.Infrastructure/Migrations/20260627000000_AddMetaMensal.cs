using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YourMoney.Infrastructure.Persistence;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260627000000_AddMetaMensal")]
    public partial class AddMetaMensal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbMetaMensal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PercentualReceita = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    MesReferencia = table.Column<DateTime>(type: "date", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbMetaMensal", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetaMensal_Usuario_MesReferencia",
                table: "tbMetaMensal",
                columns: new[] { "UsuarioId", "MesReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_tbMetaMensal_UsuarioId",
                table: "tbMetaMensal",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tbMetaMensal");
        }
    }
}
