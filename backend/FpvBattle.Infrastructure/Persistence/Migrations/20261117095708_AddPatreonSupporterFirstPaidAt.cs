using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatreonSupporterFirstPaidAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstPaidAt",
                table: "PatreonSupporters",
                type: "TEXT",
                nullable: true);

            // Everyone holding a pledge today has already been announced, including patrons
            // whose last charge was declined, so a recovered payment is not announced again.
            migrationBuilder.Sql("""
                UPDATE PatreonSupporters
                SET FirstPaidAt = FirstSupportedAt
                WHERE CAST(Amount AS REAL) > 0
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstPaidAt",
                table: "PatreonSupporters");
        }
    }
}
