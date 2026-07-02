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
            // all is exactly that broken state - add the missing "no league" marker for it,
            // dated now since the actual retirement date was never recorded.
            migrationBuilder.Sql("""
                INSERT INTO PilotLeagues (Id, PilotId, CupId, Date, League, Status)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' ||
                        substr(hex(randomblob(2)), 2) || '-' ||
                        substr('89ab', 1 + (abs(random()) % 4), 1) || substr(hex(randomblob(2)), 2) || '-' ||
                        hex(randomblob(6))),
                    PilotId,
                    CupId,
                    datetime('now'),
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
