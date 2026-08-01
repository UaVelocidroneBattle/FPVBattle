using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePilotLeagueIdCasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BackfillPilotLeagueRetirementMarkers generated its Ids via a raw-SQL
            // lower(hex(randomblob(...))) expression, so those rows' primary keys are stored as
            // all-lowercase text. Microsoft.Data.Sqlite's default Guid parameter binding renders
            // a .NET Guid as uppercase text, and SQLite's TEXT comparison is case-sensitive, so
            // EF Core can SELECT these rows fine but can never UPDATE or DELETE them by Id - the
            // WHERE Id = @p clause never matches. That surfaced as a
            // DbUpdateConcurrencyException ("expected to affect 1 row(s), but actually affected
            // 0") the moment a pilot with one of these markers needed a league change. Uppercase
            // them to match the casing EF actually writes and matches on. Only rows that are
            // currently fully lowercase are touched - anything already uppercase (i.e. every row
            // EF itself has ever written) is left untouched.
            migrationBuilder.Sql("""
                UPDATE PilotLeagues
                SET Id = upper(Id)
                WHERE Id = lower(Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
