using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxDaySreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxDayStreak",
                table: "Pilots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDayStreak",
                table: "Pilots");
        }
    }
}
