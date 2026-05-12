using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuadToQueuedTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuadId",
                table: "TrackQueue",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackQueue_QuadId",
                table: "TrackQueue",
                column: "QuadId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackQueue_QuadModels_QuadId",
                table: "TrackQueue",
                column: "QuadId",
                principalTable: "QuadModels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackQueue_QuadModels_QuadId",
                table: "TrackQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrackQueue_QuadId",
                table: "TrackQueue");

            migrationBuilder.DropColumn(
                name: "QuadId",
                table: "TrackQueue");
        }
    }
}
