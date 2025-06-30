using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveToApiDatabaseTweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackTimeDeltas_Models_DroneModelId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropIndex(
                name: "IX_TrackTimeDeltas_DroneModelId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "TrackTimes");

            migrationBuilder.DropColumn(
                name: "DroneModelId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropColumn(
                name: "UnknownDroneModelId",
                table: "TrackTimeDeltas");

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "TrackTimes",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "TrackTimeDeltas",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "TrackTimes");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "TrackTimeDeltas");

            migrationBuilder.AddColumn<int>(
                name: "ModelId",
                table: "TrackTimes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DroneModelId",
                table: "TrackTimeDeltas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnknownDroneModelId",
                table: "TrackTimeDeltas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackTimeDeltas_DroneModelId",
                table: "TrackTimeDeltas",
                column: "DroneModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackTimeDeltas_Models_DroneModelId",
                table: "TrackTimeDeltas",
                column: "DroneModelId",
                principalTable: "Models",
                principalColumn: "Id");
        }
    }
}
