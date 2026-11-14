using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "Teams",
                type: "TEXT",
                maxLength: 5,
                nullable: true,
                collation: "NOCASE");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Tag",
                table: "Teams",
                column: "Tag",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_Tag",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "Teams");
        }
    }
}
