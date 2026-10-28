using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntegrateMetasObjetivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrupoObjetivoInvestimentoId",
                table: "tbMetaMensal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArquivadoEm",
                table: "tbInvestimentoGrupoObjetivo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "tbInvestimentoGrupoObjetivo",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAlvo",
                table: "tbInvestimentoGrupoObjetivo",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorAlvo",
                table: "tbInvestimentoGrupoObjetivo",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_tbMetaMensal_Id_UsuarioId",
                table: "tbMetaMensal",
                columns: new[] { "Id", "UsuarioId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_tbInvestimento_Id_UsuarioId",
                table: "tbInvestimento",
                columns: new[] { "Id", "UsuarioId" });

            migrationBuilder.CreateTable(
                name: "tbAporteObjetivoInvestimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetaMensalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrupoObjetivoInvestimentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvestimentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MesReferencia = table.Column<DateTime>(type: "date", nullable: false),
                    DataConfirmacao = table.Column<DateTime>(type: "date", nullable: false),
                    OperacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbAporteObjetivoInvestimento", x => x.Id);
                    table.CheckConstraint("CK_AporteObjetivo_Valor", "[Valor] > 0");
                    table.ForeignKey(
                        name: "FK_tbAporteObjetivoInvestimento_tbInvestimentoGrupoObjetivo_GrupoObjetivoInvestimentoId_UsuarioId",
                        columns: x => new { x.GrupoObjetivoInvestimentoId, x.UsuarioId },
                        principalTable: "tbInvestimentoGrupoObjetivo",
                        principalColumns: new[] { "Id", "UsuarioId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbAporteObjetivoInvestimento_tbInvestimento_InvestimentoId_UsuarioId",
                        columns: x => new { x.InvestimentoId, x.UsuarioId },
                        principalTable: "tbInvestimento",
                        principalColumns: new[] { "Id", "UsuarioId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbAporteObjetivoInvestimento_tbMetaMensal_MetaMensalId_UsuarioId",
                        columns: x => new { x.MetaMensalId, x.UsuarioId },
                        principalTable: "tbMetaMensal",
                        principalColumns: new[] { "Id", "UsuarioId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbMetaMensal_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbMetaMensal",
                columns: new[] { "GrupoObjetivoInvestimentoId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbMetaMensal_UsuarioId_GrupoObjetivoInvestimentoId",
                table: "tbMetaMensal",
                columns: new[] { "UsuarioId", "GrupoObjetivoInvestimentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbInvestimentoGrupoObjetivo_UsuarioId_Ativo",
                table: "tbInvestimentoGrupoObjetivo",
                columns: new[] { "UsuarioId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_tbAporteObjetivoInvestimento_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbAporteObjetivoInvestimento",
                columns: new[] { "GrupoObjetivoInvestimentoId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbAporteObjetivoInvestimento_InvestimentoId_UsuarioId",
                table: "tbAporteObjetivoInvestimento",
                columns: new[] { "InvestimentoId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbAporteObjetivoInvestimento_MetaMensalId_UsuarioId",
                table: "tbAporteObjetivoInvestimento",
                columns: new[] { "MetaMensalId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbAporteObjetivoInvestimento_UsuarioId",
                table: "tbAporteObjetivoInvestimento",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tbAporteObjetivoInvestimento_UsuarioId_GrupoObjetivoInvestimentoId",
                table: "tbAporteObjetivoInvestimento",
                columns: new[] { "UsuarioId", "GrupoObjetivoInvestimentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbAporteObjetivoInvestimento_UsuarioId_MesReferencia",
                table: "tbAporteObjetivoInvestimento",
                columns: new[] { "UsuarioId", "MesReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_tbAporteObjetivoInvestimento_UsuarioId_OperacaoId",
                table: "tbAporteObjetivoInvestimento",
                columns: new[] { "UsuarioId", "OperacaoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbMetaMensal_tbInvestimentoGrupoObjetivo_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbMetaMensal",
                columns: new[] { "GrupoObjetivoInvestimentoId", "UsuarioId" },
                principalTable: "tbInvestimentoGrupoObjetivo",
                principalColumns: new[] { "Id", "UsuarioId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbMetaMensal_tbInvestimentoGrupoObjetivo_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbMetaMensal");

            migrationBuilder.DropTable(
                name: "tbAporteObjetivoInvestimento");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_tbMetaMensal_Id_UsuarioId",
                table: "tbMetaMensal");

            migrationBuilder.DropIndex(
                name: "IX_tbMetaMensal_GrupoObjetivoInvestimentoId_UsuarioId",
                table: "tbMetaMensal");

            migrationBuilder.DropIndex(
                name: "IX_tbMetaMensal_UsuarioId_GrupoObjetivoInvestimentoId",
                table: "tbMetaMensal");

            migrationBuilder.DropIndex(
                name: "IX_tbInvestimentoGrupoObjetivo_UsuarioId_Ativo",
                table: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_tbInvestimento_Id_UsuarioId",
                table: "tbInvestimento");

            migrationBuilder.DropColumn(
                name: "GrupoObjetivoInvestimentoId",
                table: "tbMetaMensal");

            migrationBuilder.DropColumn(
                name: "ArquivadoEm",
                table: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropColumn(
                name: "DataAlvo",
                table: "tbInvestimentoGrupoObjetivo");

            migrationBuilder.DropColumn(
                name: "ValorAlvo",
                table: "tbInvestimentoGrupoObjetivo");
        }
    }
}
