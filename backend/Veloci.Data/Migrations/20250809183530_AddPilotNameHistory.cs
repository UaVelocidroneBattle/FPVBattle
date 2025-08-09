using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPilotNameHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PilotNameHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OldName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    NewName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PilotId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotNameHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotNameHistory_Pilots_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Pilots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PilotNameHistory_PilotId",
                table: "PilotNameHistory",
                column: "PilotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PilotNameHistory");
        }
    }
}
