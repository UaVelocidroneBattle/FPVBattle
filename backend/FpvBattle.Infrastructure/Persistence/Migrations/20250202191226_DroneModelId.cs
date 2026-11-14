using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DroneModelId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackTimeDeltas_Models_DroneModelId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropIndex(
                name: "IX_TrackTimeDeltas_DroneModelId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropColumn(
                name: "DroneModelId",
                table: "TrackTimeDeltas");

            migrationBuilder.DropColumn(
                name: "UnknownDroneModelId",
                table: "TrackTimeDeltas");
        }
    }
}
