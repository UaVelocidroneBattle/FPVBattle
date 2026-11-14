using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class DomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackMaps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackResults",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    TrackId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MapId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tracks_TrackMaps_MapId",
                        column: x => x.MapId,
                        principalTable: "TrackMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackTimes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    ModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    GlobalRank = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalRank = table.Column<int>(type: "INTEGER", nullable: false),
                    TrackResultsId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackTimes_TrackResults_TrackResultsId",
                        column: x => x.TrackResultsId,
                        principalTable: "TrackResults",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Competitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    TrackId = table.Column<string>(type: "TEXT", nullable: false),
                    InitialResultsId = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentResultsId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Competitions_TrackResults_CurrentResultsId",
                        column: x => x.CurrentResultsId,
                        principalTable: "TrackResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Competitions_TrackResults_InitialResultsId",
                        column: x => x.InitialResultsId,
                        principalTable: "TrackResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Competitions_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackTimeDeltas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CompetitionId = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", nullable: false),
                    TrackTime = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeChange = table.Column<int>(type: "INTEGER", nullable: true),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    RankOld = table.Column<int>(type: "INTEGER", nullable: true),
                    LocalRank = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalRankOld = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTimeDeltas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackTimeDeltas_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_CurrentResultsId",
                table: "Competitions",
                column: "CurrentResultsId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_InitialResultsId",
                table: "Competitions",
                column: "InitialResultsId");

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_TrackId",
                table: "Competitions",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_MapId",
                table: "Tracks",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTimeDeltas_CompetitionId",
                table: "TrackTimeDeltas",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackTimes_TrackResultsId",
                table: "TrackTimes",
                column: "TrackResultsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackTimeDeltas");

            migrationBuilder.DropTable(
                name: "TrackTimes");

            migrationBuilder.DropTable(
                name: "Competitions");

            migrationBuilder.DropTable(
                name: "TrackResults");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "TrackMaps");
        }
    }
}
