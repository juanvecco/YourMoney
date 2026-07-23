using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceitasRecorrentesReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdContaFinanceira",
                table: "tbReceita",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbReceitaRecorrente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ValorPrevisto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdContaFinanceira = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Natureza = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EhSalario = table.Column<bool>(type: "bit", nullable: false),
                    ConsideraReservaEmergencia = table.Column<bool>(type: "bit", nullable: false),
                    DiaRecebimento = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "date", nullable: false),
                    DataTermino = table.Column<DateTime>(type: "date", nullable: true),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbReceitaRecorrente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbReceitaRecorrente_tbContaFinanceira_IdContaFinanceira",
                        column: x => x.IdContaFinanceira,
                        principalTable: "tbContaFinanceira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbReceitaRecorrenteOcorrencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceitaRecorrenteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MesReferencia = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReceitaConfirmadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinalizadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbReceitaRecorrenteOcorrencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbReceitaRecorrenteOcorrencia_tbReceitaRecorrente_ReceitaRecorrenteId",
                        column: x => x.ReceitaRecorrenteId,
                        principalTable: "tbReceitaRecorrente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbReceitaRecorrenteOcorrencia_tbReceita_ReceitaConfirmadaId",
                        column: x => x.ReceitaConfirmadaId,
                        principalTable: "tbReceita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receita_IdContaFinanceira",
                table: "tbReceita",
                column: "IdContaFinanceira");

            migrationBuilder.CreateIndex(
                name: "IX_ReceitaRecorrente_UsuarioId_Ativa",
                table: "tbReceitaRecorrente",
                columns: new[] { "UsuarioId", "Ativa" });

            migrationBuilder.CreateIndex(
                name: "IX_ReceitaRecorrente_UsuarioId_Periodo",
                table: "tbReceitaRecorrente",
                columns: new[] { "UsuarioId", "DataInicio", "DataTermino" });

            migrationBuilder.CreateIndex(
                name: "IX_tbReceitaRecorrente_IdContaFinanceira",
                table: "tbReceitaRecorrente",
                column: "IdContaFinanceira");

            migrationBuilder.CreateIndex(
                name: "IX_tbReceitaRecorrente_UsuarioId",
                table: "tbReceitaRecorrente",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceitaRecorrenteOcorrencia_Usuario_Mes_Status",
                table: "tbReceitaRecorrenteOcorrencia",
                columns: new[] { "UsuarioId", "MesReferencia", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tbReceitaRecorrenteOcorrencia_ReceitaConfirmadaId",
                table: "tbReceitaRecorrenteOcorrencia",
                column: "ReceitaConfirmadaId");

            migrationBuilder.CreateIndex(
                name: "IX_tbReceitaRecorrenteOcorrencia_ReceitaRecorrenteId",
                table: "tbReceitaRecorrenteOcorrencia",
                column: "ReceitaRecorrenteId");

            migrationBuilder.CreateIndex(
                name: "IX_tbReceitaRecorrenteOcorrencia_UsuarioId",
                table: "tbReceitaRecorrenteOcorrencia",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_ReceitaRecorrenteOcorrencia_Usuario_Recorrencia_Mes",
                table: "tbReceitaRecorrenteOcorrencia",
                columns: new[] { "UsuarioId", "ReceitaRecorrenteId", "MesReferencia" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbReceita_tbContaFinanceira_IdContaFinanceira",
                table: "tbReceita",
                column: "IdContaFinanceira",
                principalTable: "tbContaFinanceira",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbReceita_tbContaFinanceira_IdContaFinanceira",
                table: "tbReceita");

            migrationBuilder.DropTable(
                name: "tbReceitaRecorrenteOcorrencia");

            migrationBuilder.DropTable(
                name: "tbReceitaRecorrente");

            migrationBuilder.DropIndex(
                name: "IX_Receita_IdContaFinanceira",
                table: "tbReceita");

            migrationBuilder.DropColumn(
                name: "IdContaFinanceira",
                table: "tbReceita");
        }
    }
}
