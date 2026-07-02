using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPilotLeagueRetirementMarkers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill retirement markers for pilots demoted from all leagues before this fix
            // existed: the old code flipped their last record to Historical without adding a
            // replacement, so date-based league lookups (used by leaderboards) kept resolving
            // to their last league forever. Any (PilotId, CupId) pair with no Current record at
            // all is exactly that broken state - add the missing "no league" marker for it.
            // Dated to the start of the current month rather than "now": league updates only
            // ever run via the monthly "7 0 1 * *" job, so the real (unrecorded) retirement date
            // was always the 1st - and season leaderboards filter league lookups by
            // "Date <= start of season month", which a "now" date can fall outside of.
            migrationBuilder.Sql("""
                INSERT INTO PilotLeagues (Id, PilotId, CupId, Date, League, Status)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' ||
                        substr(hex(randomblob(2)), 2) || '-' ||
                        substr('89ab', 1 + (abs(random()) % 4), 1) || substr(hex(randomblob(2)), 2) || '-' ||
                        hex(randomblob(6))),
                    PilotId,
                    CupId,
                    datetime('now', 'start of month'),
                    NULL,
                    1
                FROM PilotLeagues
                GROUP BY PilotId, CupId
                HAVING SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
