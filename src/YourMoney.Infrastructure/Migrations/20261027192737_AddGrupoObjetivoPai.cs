using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGrupoObjetivoPai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrupoPaiId",
                table: "tbInvestimentoGrupoObjetivo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbInvestimentoGrupoObjetivo_GrupoPaiId_UsuarioId",
                table: "tbInvestimentoGrupoObjetivo",
                columns: new[] { "GrupoPaiId", "UsuarioId" });

            migrationBuilder.AddForeignKey(
                name: "FK_tbInvestimentoGrupoObjetivo_tbInvestimentoGrupoObjetivo_GrupoPaiId_UsuarioId",
                table: "tbInvestimentoGrupoObjetivo",
                columns: new[] { "GrupoPaiId", "UsuarioId" },
                principalTable: "tbInvestimentoGrupoObjetivo",
                principalColumns: new[] { "Id", "UsuarioId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbInvestimentoGrupoObjetivo_tbInvestimentoGrupoObjetivo_GrupoPaiId_UsuarioId",
                table: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropIndex(
                name: "IX_tbInvestimentoGrupoObjetivo_GrupoPaiId_UsuarioId",
                table: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropColumn(
                name: "GrupoPaiId",
                table: "tbInvestimentoGrupoObjetivo");
        }
    }
}
