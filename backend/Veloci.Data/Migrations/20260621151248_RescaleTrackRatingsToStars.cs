using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veloci.Data.Migrations
{
    /// <inheritdoc />
    public partial class RescaleTrackRatingsToStars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remap existing aggregate rating values from the old weighted-points scale (-2..3, skipping 0)
            // to the new 1–5 star scale using a linear stretch of the full range.
            // Formula: new = (old + 2) / 5 * 4 + 1  →  maps -2→1, 3→5 exactly.
            migrationBuilder.Sql(
                "UPDATE TrackRating SET Value = (Value + 2.0) / 5.0 * 4.0 + 1.0 WHERE Value IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Inverse: old = (new - 1) * 5 / 4 - 2
            migrationBuilder.Sql(
                "UPDATE TrackRating SET Value = (Value - 1.0) * 5.0 / 4.0 - 2.0 WHERE Value IS NOT NULL");
        }
    }
}
