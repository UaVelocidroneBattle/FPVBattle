using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackRatingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Tracks");

            migrationBuilder.AddColumn<string>(
                name: "RatingId",
                table: "Tracks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackRating",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PollMessageId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackRating", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_RatingId",
                table: "Tracks",
                column: "RatingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_TrackRating_RatingId",
                table: "Tracks",
                column: "RatingId",
                principalTable: "TrackRating",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_TrackRating_RatingId",
                table: "Tracks");

            migrationBuilder.DropTable(
                name: "TrackRating");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_RatingId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "RatingId",
                table: "Tracks");

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Tracks",
                type: "REAL",
                nullable: true);
        }
    }
}
