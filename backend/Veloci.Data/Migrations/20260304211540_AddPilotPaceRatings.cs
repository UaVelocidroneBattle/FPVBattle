using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPilotPaceRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PilotPaceRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PilotId = table.Column<int>(type: "INTEGER", nullable: false),
                    CupId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AverageGapPercent = table.Column<double>(type: "REAL", nullable: true),
                    CalculatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotPaceRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotPaceRatings_Pilots_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Pilots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PilotPaceRatings_PilotId_CupId",
                table: "PilotPaceRatings",
                columns: new[] { "PilotId", "CupId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PilotPaceRatings");
        }
    }
}
