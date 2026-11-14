using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BonusPointsProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BonusPoints",
                table: "CompetitionResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BonusPoints",
                table: "CompetitionResults");
        }
    }
}
