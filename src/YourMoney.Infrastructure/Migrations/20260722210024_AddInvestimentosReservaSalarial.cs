using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestimentosReservaSalarial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OperacaoId",
                table: "tbInvestimento",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceitaRecorrenteId",
                table: "tbInvestimento",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Investimento_UsuarioId_DataInvestimento",
                table: "tbInvestimento",
                columns: new[] { "UsuarioId", "DataInvestimento" });

            migrationBuilder.CreateIndex(
                name: "IX_Investimento_UsuarioId_ReceitaRecorrenteId",
                table: "tbInvestimento",
                columns: new[] { "UsuarioId", "ReceitaRecorrenteId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbInvestimento_ReceitaRecorrenteId",
                table: "tbInvestimento",
                column: "ReceitaRecorrenteId");

            migrationBuilder.CreateIndex(
                name: "UX_Investimento_UsuarioId_OperacaoId",
                table: "tbInvestimento",
                columns: new[] { "UsuarioId", "OperacaoId" },
                unique: true,
                filter: "[OperacaoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_tbInvestimento_tbReceitaRecorrente_ReceitaRecorrenteId",
                table: "tbInvestimento",
                column: "ReceitaRecorrenteId",
                principalTable: "tbReceitaRecorrente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbInvestimento_tbReceitaRecorrente_ReceitaRecorrenteId",
                table: "tbInvestimento");

            migrationBuilder.DropIndex(
                name: "IX_Investimento_UsuarioId_DataInvestimento",
                table: "tbInvestimento");

            migrationBuilder.DropIndex(
                name: "IX_Investimento_UsuarioId_ReceitaRecorrenteId",
                table: "tbInvestimento");

            migrationBuilder.DropIndex(
                name: "IX_tbInvestimento_ReceitaRecorrenteId",
                table: "tbInvestimento");

            migrationBuilder.DropIndex(
                name: "UX_Investimento_UsuarioId_OperacaoId",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "OperacaoId",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "ReceitaRecorrenteId",
                table: "tbInvestimento");
        }
    }
}
