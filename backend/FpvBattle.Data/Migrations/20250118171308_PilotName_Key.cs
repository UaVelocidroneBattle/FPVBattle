using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Data.Migrations
{
    /// <inheritdoc />
    public partial class PilotName_Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pilots",
                table: "Pilots");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Pilots");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pilots",
                table: "Pilots",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pilots",
                table: "Pilots");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Pilots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pilots",
                table: "Pilots",
                column: "Id");
        }
    }
}
