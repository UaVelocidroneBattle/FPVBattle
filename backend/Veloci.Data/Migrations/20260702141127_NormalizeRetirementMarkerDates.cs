using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRetirementMarkerDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The previous backfill migration dated retirement markers "now" instead of the
            // start of the month. Since league updates only ever run via the monthly
            // "7 0 1 * *" job, a correctly-dated marker is always on the 1st - so normalize any
            // that aren't. The date(...) != date(...) guard means this only touches rows the
            // earlier migration actually got wrong; a marker already dated on the 1st (from a
            // real job run) is left completely untouched, so this is safe to run regardless of
            // when the previous migration happened to execute.
            migrationBuilder.Sql("""
                UPDATE PilotLeagues
                SET Date = datetime(Date, 'start of month')
                WHERE League IS NULL
                  AND date(Date) != date(Date, 'start of month');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
