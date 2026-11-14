using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitionResults",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CompetitionId = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    TrackTime = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalRank = table.Column<int>(type: "INTEGER", nullable: false),
                    GlobalRank = table.Column<int>(type: "INTEGER", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionResults_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionResults_CompetitionId",
                table: "CompetitionResults",
                column: "CompetitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitionResults");
        }
    }
}
