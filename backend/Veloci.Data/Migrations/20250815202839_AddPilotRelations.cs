using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPilotRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop PlayerName columns
            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "TrackTimeDeltas");

            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "CompetitionResults");

            // Rename UserId → PilotId (keeps existing values)
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TrackTimeDeltas",
                newName: "PilotId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CompetitionResults",
                newName: "PilotId");

            // Change type from nullable to non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "PilotId",
                table: "TrackTimeDeltas",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PilotId",
                table: "CompetitionResults",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackTimeDeltas_PilotId",
                table: "TrackTimeDeltas",
                column: "PilotId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionResults_PilotId",
                table: "CompetitionResults",
                column: "PilotId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionResults_Pilots_PilotId",
                table: "CompetitionResults",
                column: "PilotId",
                principalTable: "Pilots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackTimeDeltas_Pilots_PilotId",
                table: "TrackTimeDeltas",
                column: "PilotId",
                principalTable: "Pilots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_TrackTimeDeltas_Pilots_PilotId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionResults_Pilots_PilotId",
                table: "CompetitionResults");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_TrackTimeDeltas_PilotId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionResults_PilotId",
                table: "CompetitionResults");

            // Change column back to nullable before rename
            migrationBuilder.AlterColumn<int>(
                name: "PilotId",
                table: "TrackTimeDeltas",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "PilotId",
                table: "CompetitionResults",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            // Rename PilotId → UserId
            migrationBuilder.RenameColumn(
                name: "PilotId",
                table: "TrackTimeDeltas",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PilotId",
                table: "CompetitionResults",
                newName: "UserId");

            // Add back PlayerName columns (nullable because old data may be missing)
            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "TrackTimeDeltas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "CompetitionResults",
                type: "TEXT",
                nullable: true);
        }
    }
}
