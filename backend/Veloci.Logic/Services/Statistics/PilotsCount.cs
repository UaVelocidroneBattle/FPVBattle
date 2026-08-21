namespace Veloci.Logic.Services.Statistics;

/// <summary>
/// How many pilots the community counted on each day of a continuous span.
/// </summary>
/// <param name="Days">Every calendar day of the requested window, without gaps, ascending.</param>
/// <param name="Totals">
/// Running total of pilots at the end of each day, aligned to <paramref name="Days"/>.
/// Includes everyone who joined before the window, so the line reads as the size of the
/// community rather than as growth since the window opened.
/// </param>
public record PilotsCount(IReadOnlyList<DateTime> Days, IReadOnlyList<int> Totals)
{
    public static PilotsCount Empty { get; } = new([], []);
}

/// <summary>
/// How many pilots joined in each calendar month of a continuous span.
/// </summary>
/// <param name="Months">Every month of the requested window, without gaps, ascending, as first-of-month dates.</param>
/// <param name="Counts">Pilots who joined in each month, aligned to <paramref name="Months"/>. Zero for quiet months.</param>
public record NewPilotsCount(IReadOnlyList<DateTime> Months, IReadOnlyList<int> Counts)
{
    public static NewPilotsCount Empty { get; } = new([], []);
}
