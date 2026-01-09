using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPilotIdToPatreonSupporter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PilotId",
                table: "PatreonSupporters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatreonSupporters_PilotId",
                table: "PatreonSupporters",
                column: "PilotId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatreonSupporters_Pilots_PilotId",
                table: "PatreonSupporters",
                column: "PilotId",
                principalTable: "Pilots",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatreonSupporters_Pilots_PilotId",
                table: "PatreonSupporters");

            migrationBuilder.DropIndex(
                name: "IX_PatreonSupporters_PilotId",
                table: "PatreonSupporters");

            migrationBuilder.DropColumn(
                name: "PilotId",
                table: "PatreonSupporters");
        }
    }
}
