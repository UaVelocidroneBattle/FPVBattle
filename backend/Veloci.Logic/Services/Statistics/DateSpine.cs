namespace Veloci.Logic.Services.Statistics;

/// <summary>
/// Builds the continuous axis a chart is plotted against.
/// </summary>
/// <remarks>
/// Charts read their shape from the gaps as much as from the values: a week nobody
/// joined should be a flat week, not a missing one. Grouping query results only ever
/// yields the periods that actually have data, so every series is aligned to a spine
/// built here instead of to the rows that happened to come back.
/// </remarks>
public static class DateSpine
{
    /// <summary>Every calendar day in <c>[from, to]</c>, ascending.</summary>
    public static List<DateTime> Days(DateTime from, DateTime to)
    {
        return Enumerable
            .Range(0, (to.Date - from.Date).Days + 1)
            .Select(offset => from.Date.AddDays(offset))
            .ToList();
    }

    /// <summary>Every calendar month in <c>[from, to]</c>, ascending, as first-of-month dates.</summary>
    public static List<DateTime> Months(DateTime from, DateTime to)
    {
        var first = FirstOfMonth(from);
        var months = (to.Year - from.Year) * 12 + to.Month - from.Month;

        return Enumerable
            .Range(0, months + 1)
            .Select(offset => first.AddMonths(offset))
            .ToList();
    }

    public static DateTime FirstOfMonth(DateTime day) => new(day.Year, day.Month, 1);

    /// <summary>
    /// <paramref name="day"/>, held back to <paramref name="floor"/>. Used to keep a
    /// requested range from reaching past the first day there was anything to show.
    /// </summary>
    public static DateTime NotEarlierThan(DateTime day, DateTime floor) => day > floor ? day : floor;
}
