using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigratePilotPrimaryKeyToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys = 0;");

            migrationBuilder.Sql( /*lang=sql*/@"
DROP INDEX ""IX_PilotAchievements_PilotName"";

DROP INDEX ""IX_DayStreakFreezes_PilotName"";

ALTER TABLE ""PilotAchievements"" ADD ""PilotId"" INTEGER NULL;

ALTER TABLE ""DayStreakFreezes"" ADD ""PilotId"" INTEGER NULL;

-- Populate PilotId foreign keys based on existing PilotName references
UPDATE PilotAchievements 
SET PilotId = (SELECT Id FROM Pilots WHERE Pilots.Name = PilotAchievements.PilotName)
WHERE EXISTS (SELECT 1 FROM Pilots WHERE Pilots.Name = PilotAchievements.PilotName);

UPDATE DayStreakFreezes 
SET PilotId = (SELECT Id FROM Pilots WHERE Pilots.Name = DayStreakFreezes.PilotName)
WHERE EXISTS (SELECT 1 FROM Pilots WHERE Pilots.Name = DayStreakFreezes.PilotName);

UPDATE TrackTimes 
SET UserId = (SELECT Id FROM Pilots WHERE Pilots.Name = TrackTimes.PlayerName)
WHERE EXISTS (SELECT 1 FROM Pilots WHERE Pilots.Name = TrackTimes.PlayerName);

UPDATE TrackTimeDeltas 
SET UserId = (SELECT Id FROM Pilots WHERE Pilots.Name = TrackTimeDeltas.PlayerName)
WHERE EXISTS (SELECT 1 FROM Pilots WHERE Pilots.Name = TrackTimeDeltas.PlayerName);

CREATE TABLE ""ef_temp_DayStreakFreezes"" (
    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_DayStreakFreezes"" PRIMARY KEY,
    ""CreatedOn"" TEXT NOT NULL,
    ""PilotId"" INTEGER NULL,
    ""SpentOn"" TEXT NULL
);

INSERT INTO ""ef_temp_DayStreakFreezes"" (""Id"", ""CreatedOn"", ""PilotId"", ""SpentOn"")
SELECT ""Id"", ""CreatedOn"", ""PilotId"", ""SpentOn""
FROM ""DayStreakFreezes"";

CREATE TABLE ""ef_temp_PilotAchievements"" (
    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_PilotAchievements"" PRIMARY KEY,
    ""Date"" TEXT NOT NULL,
    ""Name"" TEXT NOT NULL,
    ""PilotId"" INTEGER NULL
);

INSERT INTO ""ef_temp_PilotAchievements"" (""Id"", ""Date"", ""Name"", ""PilotId"")
SELECT ""Id"", ""Date"", ""Name"", ""PilotId""
FROM ""PilotAchievements"";

CREATE TABLE ""ef_temp_Pilots"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Pilots"" PRIMARY KEY,
    ""DayStreak"" INTEGER NOT NULL,
    ""LastRaceDate"" TEXT NULL,
    ""MaxDayStreak"" INTEGER NOT NULL,
    ""Name"" TEXT NOT NULL
);

INSERT INTO ""ef_temp_Pilots"" (""Id"", ""DayStreak"", ""LastRaceDate"", ""MaxDayStreak"", ""Name"")
SELECT IFNULL(""Id"", 0), ""DayStreak"", ""LastRaceDate"", ""MaxDayStreak"", ""Name""
FROM ""Pilots"";
");

            migrationBuilder.Sql("PRAGMA foreign_keys = 0;");

            migrationBuilder.Sql( /*lang=sql*/@"
DROP TABLE ""DayStreakFreezes"";

ALTER TABLE ""ef_temp_DayStreakFreezes"" RENAME TO ""DayStreakFreezes"";

DROP TABLE ""PilotAchievements"";

ALTER TABLE ""ef_temp_PilotAchievements"" RENAME TO ""PilotAchievements"";

DROP TABLE ""Pilots"";

ALTER TABLE ""ef_temp_Pilots"" RENAME TO ""Pilots"";
");

            migrationBuilder.Sql("PRAGMA foreign_keys = 1;");

            migrationBuilder.Sql( /*lang=sql*/@"
CREATE INDEX ""IX_DayStreakFreezes_PilotId"" ON ""DayStreakFreezes"" (""PilotId"");

CREATE INDEX ""IX_PilotAchievements_PilotId"" ON ""PilotAchievements"" (""PilotId"");
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayStreakFreezes_Pilots_PilotId",
                table: "DayStreakFreezes");

            migrationBuilder.DropForeignKey(
                name: "FK_PilotAchievements_Pilots_PilotId",
                table: "PilotAchievements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pilots",
                table: "Pilots");

            migrationBuilder.DropIndex(
                name: "IX_PilotAchievements_PilotId",
                table: "PilotAchievements");

            migrationBuilder.DropIndex(
                name: "IX_DayStreakFreezes_PilotId",
                table: "DayStreakFreezes");

            migrationBuilder.DropColumn(
                name: "PilotId",
                table: "PilotAchievements");

            migrationBuilder.DropColumn(
                name: "PilotId",
                table: "DayStreakFreezes");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Pilots",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "PilotName",
                table: "PilotAchievements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PilotName",
                table: "DayStreakFreezes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pilots",
                table: "Pilots",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PilotAchievements_PilotName",
                table: "PilotAchievements",
                column: "PilotName");

            migrationBuilder.CreateIndex(
                name: "IX_DayStreakFreezes_PilotName",
                table: "DayStreakFreezes",
                column: "PilotName");

            migrationBuilder.AddForeignKey(
                name: "FK_DayStreakFreezes_Pilots_PilotName",
                table: "DayStreakFreezes",
                column: "PilotName",
                principalTable: "Pilots",
                principalColumn: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_PilotAchievements_Pilots_PilotName",
                table: "PilotAchievements",
                column: "PilotName",
                principalTable: "Pilots",
                principalColumn: "Name");
        }
    }
}
