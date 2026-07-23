using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMetaMensalModalidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PercentualReceita",
                table: "tbMetaMensal",
                type: "decimal(9,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,4)");

            migrationBuilder.AddColumn<string>(
                name: "TipoDefinicao",
                table: "tbMetaMensal",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Percentual");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorMeta",
                table: "tbMetaMensal",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MetaMensal_Definicao",
                table: "tbMetaMensal",
                sql: "([TipoDefinicao] = N'Percentual' AND [PercentualReceita] IS NOT NULL AND [PercentualReceita] > 0 AND [ValorMeta] IS NULL) OR ([TipoDefinicao] = N'Valor' AND [ValorMeta] IS NOT NULL AND [ValorMeta] > 0 AND [PercentualReceita] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [tbMetaMensal] WHERE [TipoDefinicao] = N'Valor')
                    THROW 51000, N'Rollback bloqueado: existem metas definidas por valor.', 1;
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_MetaMensal_Definicao",
                table: "tbMetaMensal");

            migrationBuilder.DropColumn(
                name: "TipoDefinicao",
                table: "tbMetaMensal");

            migrationBuilder.DropColumn(
                name: "ValorMeta",
                table: "tbMetaMensal");

            migrationBuilder.AlterColumn<decimal>(
                name: "PercentualReceita",
                table: "tbMetaMensal",
                type: "decimal(9,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,4)",
                oldNullable: true);
        }
    }
}
