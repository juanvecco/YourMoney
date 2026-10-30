using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourMoney.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPluggyConsentExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConsentExpiresAt",
                table: "tbPluggyConnection",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentExpiresAt",
                table: "tbPluggyConnection");
        }
    }
}
