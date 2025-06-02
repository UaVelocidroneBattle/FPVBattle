using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class Achievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PilotAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotName = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotAchievements_Pilots_PilotName",
                        column: x => x.PilotName,
                        principalTable: "Pilots",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PilotAchievements_PilotName",
                table: "PilotAchievements",
                column: "PilotName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PilotAchievements");
        }
    }
}
