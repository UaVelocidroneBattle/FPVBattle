using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Services.Statistics;

/// <summary>
/// Answers "how big is the pilot community, and how fast is it growing?" for the
/// statistics dashboard.
/// </summary>
/// <remarks>
/// Both figures are read from <see cref="Pilot.CreatedAt"/>, the day a pilot first
/// posted a result. It is filled in for pilots that predate the column too, so it
/// carries the whole history rather than only the part since it was introduced.
/// </remarks>
public class PilotsCountStatisticsService
{
    private readonly IRepository<Pilot> _pilots;

    public PilotsCountStatisticsService(IRepository<Pilot> pilots)
    {
        _pilots = pilots;
    }

    /// <summary>
    /// The total pilot count on each of the last <paramref name="lastDays"/> days.
    /// Pass <c>null</c> for the whole history.
    /// </summary>
    public async Task<PilotsCount> GetPilotsCountStatistics(int? lastDays)
    {
        var joinsPerDay = await JoinsPerDayAsync();

        if (joinsPerDay.Count == 0)
            return PilotsCount.Empty;

        var from = DateSpine.NotEarlierThan(WindowStart(lastDays), floor: joinsPerDay.Keys.Min());
        var days = DateSpine.Days(from, DateTime.Today);

        // Pilots who joined before the window are what the line starts from. Without them
        // it would open at zero and read as though the community had just been founded.
        var total = joinsPerDay
            .Where(join => join.Key < from)
            .Sum(join => join.Value);

        var totals = new List<int>(days.Count);

        foreach (var day in days)
        {
            total += joinsPerDay.GetValueOrDefault(day);
            totals.Add(total);
        }

        return new PilotsCount(days, totals);
    }

    /// <summary>
    /// Pilots who joined in each of the last <paramref name="lastMonths"/> calendar months,
    /// the current one included. Pass <c>null</c> for the whole history.
    /// </summary>
    public async Task<NewPilotsCount> GetNewPilotsCountStatistics(int? lastMonths)
    {
        var joinsPerMonth = await JoinsPerMonthAsync();

        if (joinsPerMonth.Count == 0)
            return NewPilotsCount.Empty;

        var thisMonth = DateSpine.FirstOfMonth(DateTime.Today);
        var requested = lastMonths is null ? DateTime.MinValue : thisMonth.AddMonths(1 - lastMonths.Value);
        var from = DateSpine.NotEarlierThan(requested, floor: joinsPerMonth.Keys.Min());

        var months = DateSpine.Months(from, thisMonth);

        return new NewPilotsCount(
            months,
            months.Select(month => joinsPerMonth.GetValueOrDefault(month)).ToList());
    }

    /// <summary>
    /// The first day of the requested window, or <see cref="DateTime.MinValue"/> for the
    /// whole history. Counting <paramref name="lastDays"/> inclusively of today keeps
    /// "30 days" thirty points wide rather than thirty-one.
    /// </summary>
    private static DateTime WindowStart(int? lastDays)
    {
        return lastDays is null ? DateTime.MinValue : DateTime.Today.AddDays(1 - lastDays.Value);
    }

    /// <remarks>
    /// Grouped in the database: the dashboard only ever needs the counts, and a pilot
    /// row is far wider than the one date this reads from it.
    /// </remarks>
    private async Task<Dictionary<DateTime, int>> JoinsPerDayAsync()
    {
        var joins = await _pilots.GetAll()
            .GroupBy(pilot => pilot.CreatedAt.Date)
            .Select(group => new { Day = group.Key, Count = group.Count() })
            .ToListAsync();

        return joins.ToDictionary(join => join.Day, join => join.Count);
    }

    private async Task<Dictionary<DateTime, int>> JoinsPerMonthAsync()
    {
        var joins = await _pilots.GetAll()
            .GroupBy(pilot => new { pilot.CreatedAt.Year, pilot.CreatedAt.Month })
            .Select(group => new { group.Key.Year, group.Key.Month, Count = group.Count() })
            .ToListAsync();

        return joins.ToDictionary(join => new DateTime(join.Year, join.Month, 1), join => join.Count);
    }
}
