using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YourMoney.Infrastructure.Persistence;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260706000000_AddDespesasRecorrentes")]
    public partial class AddDespesasRecorrentes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbDespesaRecorrente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ValorPrevisto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdContaFinanceira = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTipoDespesa = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdNaturezaDespesa = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCategoria = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiaVencimento = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataTermino = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbDespesaRecorrente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbDespesaRecorrente_tbCategoria_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "tbCategoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbDespesaRecorrente_tbCategoria_IdNaturezaDespesa",
                        column: x => x.IdNaturezaDespesa,
                        principalTable: "tbCategoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbDespesaRecorrente_tbCategoria_IdTipoDespesa",
                        column: x => x.IdTipoDespesa,
                        principalTable: "tbCategoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbDespesaRecorrente_tbContaFinanceira_IdContaFinanceira",
                        column: x => x.IdContaFinanceira,
                        principalTable: "tbContaFinanceira",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbDespesaRecorrenteOcorrencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DespesaRecorrenteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MesReferencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DespesaConfirmadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinalizadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbDespesaRecorrenteOcorrencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbDespesaRecorrenteOcorrencia_tbDespesaRecorrente_DespesaRecorrenteId",
                        column: x => x.DespesaRecorrenteId,
                        principalTable: "tbDespesaRecorrente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbDespesaRecorrenteOcorrencia_tbDespesa_DespesaConfirmadaId",
                        column: x => x.DespesaConfirmadaId,
                        principalTable: "tbDespesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrente_IdCategoria",
                table: "tbDespesaRecorrente",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrente_IdContaFinanceira",
                table: "tbDespesaRecorrente",
                column: "IdContaFinanceira");

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrente_IdNaturezaDespesa",
                table: "tbDespesaRecorrente",
                column: "IdNaturezaDespesa");

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrente_IdTipoDespesa",
                table: "tbDespesaRecorrente",
                column: "IdTipoDespesa");

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrente_UsuarioId",
                table: "tbDespesaRecorrente",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrente_UsuarioId_Ativa",
                table: "tbDespesaRecorrente",
                columns: new[] { "UsuarioId", "Ativa" });

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrente_UsuarioId_Periodo",
                table: "tbDespesaRecorrente",
                columns: new[] { "UsuarioId", "DataInicio", "DataTermino" });

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrenteOcorrencia_DespesaConfirmadaId",
                table: "tbDespesaRecorrenteOcorrencia",
                column: "DespesaConfirmadaId");

            migrationBuilder.CreateIndex(
                name: "IX_tbDespesaRecorrenteOcorrencia_DespesaRecorrenteId",
                table: "tbDespesaRecorrenteOcorrencia",
                column: "DespesaRecorrenteId");

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrenteOcorrencia_UsuarioId",
                table: "tbDespesaRecorrenteOcorrencia",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DespesaRecorrenteOcorrencia_Usuario_Mes_Status",
                table: "tbDespesaRecorrenteOcorrencia",
                columns: new[] { "UsuarioId", "MesReferencia", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_DespesaRecorrenteOcorrencia_Usuario_Recorrencia_Mes",
                table: "tbDespesaRecorrenteOcorrencia",
                columns: new[] { "UsuarioId", "DespesaRecorrenteId", "MesReferencia" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tbDespesaRecorrenteOcorrencia");
            migrationBuilder.DropTable(name: "tbDespesaRecorrente");
        }
    }
}
