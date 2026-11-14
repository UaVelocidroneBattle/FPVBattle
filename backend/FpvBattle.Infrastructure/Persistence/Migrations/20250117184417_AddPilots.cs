using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPilots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pilots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastRaceDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DayStreak = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pilots", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pilots");
        }
    }
}
