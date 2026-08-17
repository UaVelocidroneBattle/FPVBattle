using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Services.Statistics;

/// <summary>
/// Answers "how many pilots flew each day, in each cup?" for the admin statistics page.
/// </summary>
public class ParticipationStatisticsService
{
    private readonly IRepository<Competition> _competitions;
    private readonly ICupService _cupService;

    public ParticipationStatisticsService(IRepository<Competition> competitions, ICupService cupService)
    {
        _competitions = competitions;
        _cupService = cupService;
    }

    /// <summary>
    /// Returns participation over the last <paramref name="lastDays"/> days, one series
    /// per configured cup. Pass <c>null</c> for the whole history.
    /// </summary>
    /// <remarks>
    /// Only closed competitions are counted: a competition still in progress has no
    /// <see cref="Competition.CompetitionResults"/> yet and would read as a day nobody flew.
    /// Every pilot has at most one result per competition, so the result count *is* the
    /// number of participating pilots.
    /// </remarks>
    public async Task<DailyParticipation> GetDailyParticipationAsync(int? lastDays)
    {
        var competitions = await CompetitionsQuery(lastDays)
            .Select(comp => new
            {
                comp.CupId,
                comp.StartedOn,
                PilotCount = comp.CompetitionResults.Count
            })
            .ToListAsync();

        if (competitions.Count == 0)
            return DailyParticipation.Empty;

        var pilotsPerCupPerDay = competitions
            .GroupBy(comp => (comp.CupId, Day: comp.StartedOn.Date))
            .ToDictionary(group => group.Key, group => group.Sum(comp => comp.PilotCount));

        var days = DateSpine.Days(
            from: pilotsPerCupPerDay.Keys.Min(key => key.Day),
            to: pilotsPerCupPerDay.Keys.Max(key => key.Day));

        var series = _cupService.GetAllCups()
            .Select(cup => new CupParticipationSeries(
                CupId: cup.Key,
                CupName: cup.Value.Name,
                Counts: days
                    .Select(day => pilotsPerCupPerDay.TryGetValue((cup.Key, day), out var pilots) ? pilots : (int?)null)
                    .ToList()))
            .ToList();

        return new DailyParticipation(days, series);
    }

    /// <summary>
    /// Closed competitions of the requested window. Narrowing the window in SQL keeps the
    /// per-competition result count off the years of history the reader is not looking at;
    /// <c>Competitions.StartedOn</c> is indexed, so the window itself is cheap to take.
    /// </summary>
    private IQueryable<Competition> CompetitionsQuery(int? lastDays)
    {
        var competitions = _competitions.GetAll().Closed();

        if (lastDays is null)
            return competitions;

        var today = DateTime.Today;

        return competitions.InRange(from: today.AddDays(1 - lastDays.Value), to: today.AddDays(1));
    }
}
