using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModelNameToCompetitionResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "CompetitionResults",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "CompetitionResults");
        }
    }
}
