using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using YourMoney.Infrastructure.Persistence;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260527000000_AddDespesaParcelamento")]
    public partial class AddDespesaParcelamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroParcela",
                table: "tbDespesa",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParcelamentoId",
                table: "tbDespesa",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalParcelas",
                table: "tbDespesa",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotalParcelamento",
                table: "tbDespesa",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Despesa_Parcelamento_Parcela",
                table: "tbDespesa",
                columns: new[] { "ParcelamentoId", "NumeroParcela" });

            migrationBuilder.CreateIndex(
                name: "IX_Despesa_ParcelamentoId",
                table: "tbDespesa",
                column: "ParcelamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Despesa_Parcelamento_Parcela",
                table: "tbDespesa");

            migrationBuilder.DropIndex(
                name: "IX_Despesa_ParcelamentoId",
                table: "tbDespesa");

            migrationBuilder.DropColumn(
                name: "NumeroParcela",
                table: "tbDespesa");

            migrationBuilder.DropColumn(
                name: "ParcelamentoId",
                table: "tbDespesa");

            migrationBuilder.DropColumn(
                name: "TotalParcelas",
                table: "tbDespesa");

            migrationBuilder.DropColumn(
                name: "ValorTotalParcelamento",
                table: "tbDespesa");
        }
    }
}
