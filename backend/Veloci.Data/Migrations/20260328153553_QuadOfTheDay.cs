using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class QuadOfTheDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuadOfTheDayId",
                table: "Competitions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_QuadOfTheDayId",
                table: "Competitions",
                column: "QuadOfTheDayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Competitions_QuadModels_QuadOfTheDayId",
                table: "Competitions",
                column: "QuadOfTheDayId",
                principalTable: "QuadModels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Competitions_QuadModels_QuadOfTheDayId",
                table: "Competitions");

            migrationBuilder.DropIndex(
                name: "IX_Competitions_QuadOfTheDayId",
                table: "Competitions");

            migrationBuilder.DropColumn(
                name: "QuadOfTheDayId",
                table: "Competitions");
        }
    }
}
