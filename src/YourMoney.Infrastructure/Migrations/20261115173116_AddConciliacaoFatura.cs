using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConciliacaoFatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Conferida",
                table: "tbDespesa",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "tbFaturaConta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdContaFinanceira = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MesReferencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorInformado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbFaturaConta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbFaturaConta_tbContaFinanceira_IdContaFinanceira",
                        column: x => x.IdContaFinanceira,
                        principalTable: "tbContaFinanceira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbFaturaConta_IdContaFinanceira",
                table: "tbFaturaConta",
                column: "IdContaFinanceira");

            migrationBuilder.CreateIndex(
                name: "IX_tbFaturaConta_UsuarioId",
                table: "tbFaturaConta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tbFaturaConta_UsuarioId_IdContaFinanceira_MesReferencia",
                table: "tbFaturaConta",
                columns: new[] { "UsuarioId", "IdContaFinanceira", "MesReferencia" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbFaturaConta");

            migrationBuilder.DropColumn(
                name: "Conferida",
                table: "tbDespesa");
        }
    }
}
