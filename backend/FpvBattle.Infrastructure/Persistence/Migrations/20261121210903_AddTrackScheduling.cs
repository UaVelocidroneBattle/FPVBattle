using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A track waits in the queue at most once: an older duplicate keeps its place, later copies are removed
            migrationBuilder.Sql("""
                DELETE FROM "TrackQueue"
                WHERE "Used" = 0
                  AND EXISTS (SELECT 1 FROM "TrackQueue" AS earlier
                              WHERE earlier."TrackId" = "TrackQueue"."TrackId"
                                AND earlier."Used" = 0
                                AND (earlier."AddedOn" < "TrackQueue"."AddedOn"
                                     OR (earlier."AddedOn" = "TrackQueue"."AddedOn" AND earlier."Id" < "TrackQueue"."Id")));
                """);

            migrationBuilder.DropIndex(
                name: "IX_TrackQueue_TrackId",
                table: "TrackQueue");

            migrationBuilder.AddColumn<string>(
                name: "ScheduledByUserId",
                table: "TrackQueue",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackQueue_ScheduledByUserId_AddedOn",
                table: "TrackQueue",
                columns: new[] { "ScheduledByUserId", "AddedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackQueue_TrackId",
                table: "TrackQueue",
                column: "TrackId",
                unique: true,
                filter: "\"Used\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackQueue_AspNetUsers_ScheduledByUserId",
                table: "TrackQueue",
                column: "ScheduledByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackQueue_AspNetUsers_ScheduledByUserId",
                table: "TrackQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrackQueue_ScheduledByUserId_AddedOn",
                table: "TrackQueue");

            migrationBuilder.DropIndex(
                name: "IX_TrackQueue_TrackId",
                table: "TrackQueue");

            migrationBuilder.DropColumn(
                name: "ScheduledByUserId",
                table: "TrackQueue");

            migrationBuilder.CreateIndex(
                name: "IX_TrackQueue_TrackId",
                table: "TrackQueue",
                column: "TrackId");
        }
    }
}
