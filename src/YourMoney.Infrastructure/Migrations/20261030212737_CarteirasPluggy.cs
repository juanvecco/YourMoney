using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CarteirasPluggy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbCarteira",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NomeNormalizado = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Principal = table.Column<bool>(type: "bit", nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AtualizadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbCarteira", x => x.Id);
                    table.UniqueConstraint("AK_tbCarteira_Id_UsuarioId", x => new { x.Id, x.UsuarioId });
                });

            // Build a principal wallet for every owner represented by legacy Open Finance data.
            migrationBuilder.Sql("""
                INSERT INTO [tbCarteira] ([Id], [Nome], [NomeNormalizado], [Principal], [CriadaEm], [AtualizadaEm], [UsuarioId])
                SELECT NEWID(), N'Minha carteira', N'MINHA CARTEIRA', 1, SYSUTCDATETIME(), NULL, owners.[UsuarioId]
                FROM (
                    SELECT [UsuarioId] FROM [tbContaFinanceira]
                    UNION
                    SELECT [UsuarioId] FROM [tbPluggyCredential]
                    UNION
                    SELECT [UsuarioId] FROM [tbPluggyConnection]
                ) AS owners;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "CarteiraId",
                table: "tbPluggyCredential",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CarteiraId",
                table: "tbPluggyConnection",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CarteiraId",
                table: "tbContaFinanceira",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE account SET [CarteiraId] = wallet.[Id]
                FROM [tbContaFinanceira] AS account
                INNER JOIN [tbCarteira] AS wallet ON wallet.[UsuarioId] = account.[UsuarioId] AND wallet.[Principal] = 1;

                UPDATE credential SET [CarteiraId] = wallet.[Id]
                FROM [tbPluggyCredential] AS credential
                INNER JOIN [tbCarteira] AS wallet ON wallet.[UsuarioId] = credential.[UsuarioId] AND wallet.[Principal] = 1;

                UPDATE connection SET [CarteiraId] = wallet.[Id]
                FROM [tbPluggyConnection] AS connection
                INNER JOIN [tbCarteira] AS wallet ON wallet.[UsuarioId] = connection.[UsuarioId] AND wallet.[Principal] = 1;
                """);

            migrationBuilder.AlterColumn<Guid>(name: "CarteiraId", table: "tbPluggyCredential", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
            migrationBuilder.AlterColumn<Guid>(name: "CarteiraId", table: "tbPluggyConnection", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
            migrationBuilder.AlterColumn<Guid>(name: "CarteiraId", table: "tbContaFinanceira", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.DropIndex(name: "IX_tbPluggyCredential_UsuarioId", table: "tbPluggyCredential");

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyCredential_CarteiraId",
                table: "tbPluggyCredential",
                column: "CarteiraId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyCredential_CarteiraId_UsuarioId",
                table: "tbPluggyCredential",
                columns: new[] { "CarteiraId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyCredential_UsuarioId",
                table: "tbPluggyCredential",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyConnection_CarteiraId_UsuarioId",
                table: "tbPluggyConnection",
                columns: new[] { "CarteiraId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyConnection_UsuarioId_CarteiraId",
                table: "tbPluggyConnection",
                columns: new[] { "UsuarioId", "CarteiraId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbContaFinanceira_CarteiraId_UsuarioId",
                table: "tbContaFinanceira",
                columns: new[] { "CarteiraId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbContaFinanceira_UsuarioId_CarteiraId",
                table: "tbContaFinanceira",
                columns: new[] { "UsuarioId", "CarteiraId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbCarteira_UsuarioId",
                table: "tbCarteira",
                column: "UsuarioId",
                unique: true,
                filter: "[Principal] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_tbCarteira_UsuarioId_NomeNormalizado",
                table: "tbCarteira",
                columns: new[] { "UsuarioId", "NomeNormalizado" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbContaFinanceira_tbCarteira_CarteiraId_UsuarioId",
                table: "tbContaFinanceira",
                columns: new[] { "CarteiraId", "UsuarioId" },
                principalTable: "tbCarteira",
                principalColumns: new[] { "Id", "UsuarioId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbPluggyConnection_tbCarteira_CarteiraId_UsuarioId",
                table: "tbPluggyConnection",
                columns: new[] { "CarteiraId", "UsuarioId" },
                principalTable: "tbCarteira",
                principalColumns: new[] { "Id", "UsuarioId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbPluggyCredential_tbCarteira_CarteiraId_UsuarioId",
                table: "tbPluggyCredential",
                columns: new[] { "CarteiraId", "UsuarioId" },
                principalTable: "tbCarteira",
                principalColumns: new[] { "Id", "UsuarioId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [tbCarteira] WHERE [Principal] = 0)
                    THROW 51000, 'Cannot revert CarteirasPluggy while secondary wallets exist.', 1;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_tbContaFinanceira_tbCarteira_CarteiraId_UsuarioId",
                table: "tbContaFinanceira");

            migrationBuilder.DropForeignKey(
                name: "FK_tbPluggyConnection_tbCarteira_CarteiraId_UsuarioId",
                table: "tbPluggyConnection");

            migrationBuilder.DropForeignKey(
                name: "FK_tbPluggyCredential_tbCarteira_CarteiraId_UsuarioId",
                table: "tbPluggyCredential");

            migrationBuilder.DropTable(
                name: "tbCarteira");

            migrationBuilder.DropIndex(
                name: "IX_tbPluggyCredential_CarteiraId",
                table: "tbPluggyCredential");

            migrationBuilder.DropIndex(
                name: "IX_tbPluggyCredential_CarteiraId_UsuarioId",
                table: "tbPluggyCredential");

            migrationBuilder.DropIndex(
                name: "IX_tbPluggyCredential_UsuarioId",
                table: "tbPluggyCredential");

            migrationBuilder.DropIndex(
                name: "IX_tbPluggyConnection_CarteiraId_UsuarioId",
                table: "tbPluggyConnection");

            migrationBuilder.DropIndex(
                name: "IX_tbPluggyConnection_UsuarioId_CarteiraId",
                table: "tbPluggyConnection");

            migrationBuilder.DropIndex(
                name: "IX_tbContaFinanceira_CarteiraId_UsuarioId",
                table: "tbContaFinanceira");

            migrationBuilder.DropIndex(
                name: "IX_tbContaFinanceira_UsuarioId_CarteiraId",
                table: "tbContaFinanceira");

            migrationBuilder.DropColumn(
                name: "CarteiraId",
                table: "tbPluggyCredential");

            migrationBuilder.DropColumn(
                name: "CarteiraId",
                table: "tbPluggyConnection");

            migrationBuilder.DropColumn(
                name: "CarteiraId",
                table: "tbContaFinanceira");

            migrationBuilder.DropTable(name: "tbCarteira");

            migrationBuilder.CreateIndex(
                name: "IX_tbPluggyCredential_UsuarioId",
                table: "tbPluggyCredential",
                column: "UsuarioId",
                unique: true);
        }
    }
}
