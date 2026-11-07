using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegrasNegocioUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbRegraNegocioUsuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    AtualizadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbRegraNegocioUsuario", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbRegraNegocioUsuario_UsuarioId",
                table: "tbRegraNegocioUsuario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tbRegraNegocioUsuario_UsuarioId_Codigo",
                table: "tbRegraNegocioUsuario",
                columns: new[] { "UsuarioId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbRegraNegocioUsuario");
        }
    }
}
