using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeagueHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Qualification",
                table: "Pilots",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LeagueHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OldLeagueId = table.Column<int>(type: "INTEGER", nullable: true),
                    NewLeagueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PilotId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueHistory_Leagues_NewLeagueId",
                        column: x => x.NewLeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueHistory_Leagues_OldLeagueId",
                        column: x => x.OldLeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeagueHistory_Pilots_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Pilots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueHistory_NewLeagueId",
                table: "LeagueHistory",
                column: "NewLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueHistory_OldLeagueId",
                table: "LeagueHistory",
                column: "OldLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueHistory_PilotId",
                table: "LeagueHistory",
                column: "PilotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeagueHistory");

            migrationBuilder.DropColumn(
                name: "Qualification",
                table: "Pilots");
        }
    }
}
