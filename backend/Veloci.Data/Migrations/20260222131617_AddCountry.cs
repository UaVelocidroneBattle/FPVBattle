using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TrackTimes",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "UA");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TrackTimeDeltas",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "UA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "TrackTimes");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "TrackTimeDeltas");
        }
    }
}
