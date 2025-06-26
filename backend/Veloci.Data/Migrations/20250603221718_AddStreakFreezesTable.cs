using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakFreezesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayStreakFreezes",
                table: "Pilots");

            migrationBuilder.CreateTable(
                name: "DayStreakFreezes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotName = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SpentOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayStreakFreezes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayStreakFreezes_Pilots_PilotName",
                        column: x => x.PilotName,
                        principalTable: "Pilots",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayStreakFreezes_PilotName",
                table: "DayStreakFreezes",
                column: "PilotName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayStreakFreezes");

            migrationBuilder.AddColumn<int>(
                name: "DayStreakFreezes",
                table: "Pilots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
