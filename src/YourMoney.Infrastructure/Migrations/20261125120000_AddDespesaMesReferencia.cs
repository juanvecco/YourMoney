using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDespesaMesReferencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MesReferencia",
                table: "tbDespesa",
                type: "date",
                nullable: true);

            if (ActiveProvider.Contains("Npgsql", StringComparison.Ordinal))
            {
                migrationBuilder.Sql("UPDATE \"tbDespesa\" SET \"MesReferencia\" = date_trunc('month', \"Data\")::date WHERE \"MesReferencia\" IS NULL;");
            }
            else
            {
                migrationBuilder.Sql("UPDATE [tbDespesa] SET [MesReferencia] = DATEFROMPARTS(YEAR([Data]), MONTH([Data]), 1) WHERE [MesReferencia] IS NULL;");
            }

            migrationBuilder.AlterColumn<DateTime>(
                name: "MesReferencia",
                table: "tbDespesa",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Despesa_UsuarioId_MesReferencia",
                table: "tbDespesa",
                columns: new[] { "UsuarioId", "MesReferencia" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Despesa_UsuarioId_MesReferencia",
                table: "tbDespesa");

            migrationBuilder.DropColumn(
                name: "MesReferencia",
                table: "tbDespesa");
        }
    }
}
