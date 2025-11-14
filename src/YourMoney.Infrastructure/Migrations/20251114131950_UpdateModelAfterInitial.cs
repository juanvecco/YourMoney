using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelAfterInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbDespesa_tbCategoria_IdCategoria",
                table: "tbDespesa");

            migrationBuilder.DropForeignKey(
                name: "FK_tbDespesa_tbContaFinanceira_IdContaFinanceira",
                table: "tbDespesa");

            migrationBuilder.DropTable(
                name: "tbContaFinanceira");

            migrationBuilder.DropTable(
                name: "tbInvestimento");

            migrationBuilder.DropTable(
                name: "tbMeta");

            migrationBuilder.DropTable(
                name: "tbReceita");

            migrationBuilder.DropTable(
                name: "tbCategoria");

            migrationBuilder.DropIndex(
                name: "IX_Despesa_IdCategoria",
                table: "tbDespesa");

            migrationBuilder.DropIndex(
                name: "IX_Despesa_IdContaFinanceira",
                table: "tbDespesa");

            migrationBuilder.DropColumn(
                name: "IdCategoria",
                table: "tbDespesa");

            migrationBuilder.DropColumn(
                name: "IdContaFinanceira",
                table: "tbDespesa");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "tbDespesa",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
