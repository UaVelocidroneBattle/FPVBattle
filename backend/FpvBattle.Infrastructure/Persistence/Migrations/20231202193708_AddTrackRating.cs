using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Tracks",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Tracks");
        }
    }
}
