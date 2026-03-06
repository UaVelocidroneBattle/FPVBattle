using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRankChangeAndPaceChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageGapChange",
                table: "PilotPaceRatings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "PilotPaceRatings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RankChange",
                table: "PilotPaceRatings",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageGapChange",
                table: "PilotPaceRatings");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "PilotPaceRatings");

            migrationBuilder.DropColumn(
                name: "RankChange",
                table: "PilotPaceRatings");
        }
    }
}
