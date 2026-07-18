using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCreatedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Backfill accounts that existed before this column was introduced
            migrationBuilder.Sql("UPDATE AspNetUsers SET CreatedOn = CURRENT_TIMESTAMP;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "AspNetUsers");
        }
    }
}
