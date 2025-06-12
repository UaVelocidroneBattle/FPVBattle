using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitionVariables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CompetitionId = table.Column<string>(type: "TEXT", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", nullable: false),
                    IntValue = table.Column<int>(type: "INTEGER", nullable: false),
                    ULongValue = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DoubleValue = table.Column<double>(type: "REAL", nullable: false),
                    BoolValue = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionVariables_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionVariables_CompetitionId",
                table: "CompetitionVariables",
                column: "CompetitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitionVariables");
        }
    }
}
