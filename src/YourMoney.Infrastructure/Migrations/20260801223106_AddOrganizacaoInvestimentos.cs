using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizacaoInvestimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrupoObjetivoInvestimentoId",
                table: "tbInvestimento",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAtivo",
                table: "tbInvestimento",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbInvestimentoGrupoObjetivo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NomeNormalizado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Versao = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbInvestimentoGrupoObjetivo", x => x.Id);
                    table.UniqueConstraint("AK_tbInvestimentoGrupoObjetivo_Id_UsuarioId", x => new { x.Id, x.UsuarioId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Investimento_UsuarioId_GrupoObjetivoId",
                table: "tbInvestimento",
                columns: new[] { "UsuarioId", "GrupoObjetivoInvestimentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Investimento_UsuarioId_TipoAtivo",
                table: "tbInvestimento",
                columns: new[] { "UsuarioId", "TipoAtivo" });

            migrationBuilder.CreateIndex(
                name: "IX_tbInvestimento_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbInvestimento",
                columns: new[] { "GrupoObjetivoInvestimentoId", "UsuarioId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Investimento_TipoAtivo",
                table: "tbInvestimento",
                sql: "[TipoAtivo] IS NULL OR [TipoAtivo] IN ('FundoInvestimento','Exterior','Criptomoeda','RendaFixa','Acoes','Outros')");

            migrationBuilder.CreateIndex(
                name: "IX_tbInvestimentoGrupoObjetivo_UsuarioId",
                table: "tbInvestimentoGrupoObjetivo",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_InvestimentoGrupoObjetivo_UsuarioId_NomeNormalizado",
                table: "tbInvestimentoGrupoObjetivo",
                columns: new[] { "UsuarioId", "NomeNormalizado" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbInvestimento_tbInvestimentoGrupoObjetivo_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbInvestimento",
                columns: new[] { "GrupoObjetivoInvestimentoId", "UsuarioId" },
                principalTable: "tbInvestimentoGrupoObjetivo",
                principalColumns: new[] { "Id", "UsuarioId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbInvestimento_tbInvestimentoGrupoObjetivo_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbInvestimento");

            migrationBuilder.DropTable(
                name: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropIndex(
                name: "IX_Investimento_UsuarioId_GrupoObjetivoId",
                table: "tbInvestimento");

            migrationBuilder.DropIndex(
                name: "IX_Investimento_UsuarioId_TipoAtivo",
                table: "tbInvestimento");

            migrationBuilder.DropIndex(
                name: "IX_tbInvestimento_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbInvestimento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Investimento_TipoAtivo",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "GrupoObjetivoInvestimentoId",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "TipoAtivo",
                table: "tbInvestimento");
        }
    }
}
