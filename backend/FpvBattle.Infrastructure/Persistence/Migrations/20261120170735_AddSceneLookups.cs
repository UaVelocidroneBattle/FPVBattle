using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSceneLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Velocidrone scene ids become unique: each duplicate merges into the copy with the most tracks
            migrationBuilder.Sql("""
                CREATE TEMP TABLE "SceneMerges" AS
                SELECT duplicate."Id" AS "DuplicateId",
                       (SELECT keeper."Id"
                        FROM "TrackMaps" AS keeper
                        WHERE keeper."MapId" = duplicate."MapId"
                        ORDER BY (SELECT COUNT(*) FROM "Tracks" WHERE "Tracks"."MapId" = keeper."Id") DESC, keeper."Id"
                        LIMIT 1) AS "KeptId"
                FROM "TrackMaps" AS duplicate;

                DELETE FROM "SceneMerges" WHERE "DuplicateId" = "KeptId";

                UPDATE "Tracks"
                SET "MapId" = (SELECT "KeptId" FROM "SceneMerges" WHERE "DuplicateId" = "Tracks"."MapId")
                WHERE "MapId" IN (SELECT "DuplicateId" FROM "SceneMerges");

                DELETE FROM "TrackMaps" WHERE "Id" IN (SELECT "DuplicateId" FROM "SceneMerges");

                DROP TABLE "SceneMerges";
                """);

            migrationBuilder.RenameTable(
                name: "TrackMaps",
                newName: "Scenes");

            migrationBuilder.RenameColumn(
                name: "MapId",
                table: "Scenes",
                newName: "SceneId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Scenes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_SceneId",
                table: "Scenes",
                column: "SceneId",
                unique: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_TrackMaps_MapId",
                table: "Tracks");

            migrationBuilder.RenameColumn(
                name: "MapId",
                table: "Tracks",
                newName: "SceneId");

            migrationBuilder.RenameIndex(
                name: "IX_Tracks_MapId",
                table: "Tracks",
                newName: "IX_Tracks_SceneId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Tracks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_TrackId",
                table: "Tracks",
                column: "TrackId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_Scenes_SceneId",
                table: "Tracks",
                column: "SceneId",
                principalTable: "Scenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Merged duplicate scenes stay merged: their tracks keep pointing at the kept scene
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_Scenes_SceneId",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_TrackId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tracks");

            migrationBuilder.RenameColumn(
                name: "SceneId",
                table: "Tracks",
                newName: "MapId");

            migrationBuilder.RenameIndex(
                name: "IX_Tracks_SceneId",
                table: "Tracks",
                newName: "IX_Tracks_MapId");

            migrationBuilder.DropIndex(
                name: "IX_Scenes_SceneId",
                table: "Scenes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Scenes");

            migrationBuilder.RenameColumn(
                name: "SceneId",
                table: "Scenes",
                newName: "MapId");

            migrationBuilder.RenameTable(
                name: "Scenes",
                newName: "TrackMaps");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_TrackMaps_MapId",
                table: "Tracks",
                column: "MapId",
                principalTable: "TrackMaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
