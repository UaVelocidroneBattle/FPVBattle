using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Leagues.Models;

namespace Veloci.Logic.Features.Leagues.Services;

public class RatingQualificationService
{
    private readonly IRepository<Competition> _competitions;
    private readonly PaceRatingSettings _settings;

    public RatingQualificationService(IRepository<Competition> competitions, IOptions<PaceRatingSettings> settings)
    {
        _competitions = competitions;
        _settings = settings.Value;
    }

    public async Task<CupRatingQualifications> GetForCupAsync(string cupId)
    {
        var today = DateTime.Today;
        var nextDistribution = new DateTime(today.Year, today.Month, 1).AddMonths(1);
        var windowStart = _settings.LookBackPeriod.StartOfWindowEndingAt(nextDistribution);

        var competitions = await _competitions.GetAll()
            .ForCup(cupId)
            .InRange(windowStart, nextDistribution)
            .Closed()
            .Include(c => c.CompetitionResults)
            .Include(c => c.QuadOfTheDay)
            .ToListAsync();

        // Today's race is still open, so it counts as a chance left rather than a track flown.
        var racingDaysLeft = (nextDistribution - today).Days;

        var byPilot = competitions
            .SelectMany(c => c.RatingEligibleResults.Select(r => (r.PilotId, Day: c.StartedOn.Date)))
            .GroupBy(x => x.PilotId)
            .ToDictionary(
                g => g.Key,
                g => Evaluate(g.Select(x => x.Day).ToList(), today, racingDaysLeft));

        return new CupRatingQualifications(
            _settings.MinDaysForRelevance,
            Evaluate([], today, racingDaysLeft),
            byPilot);
    }

    private RatingQualification Evaluate(IReadOnlyCollection<DateTime> daysFlown, DateTime today, int racingDaysLeft)
    {
        // A pilot whose race today has already closed cannot fly that day a second time.
        var daysStillOpen = daysFlown.Contains(today) ? racingDaysLeft - 1 : racingDaysLeft;

        var status = daysFlown.Count >= _settings.MinDaysForRelevance
            ? RatingQualificationStatus.Qualified
            : daysFlown.Count + daysStillOpen >= _settings.MinDaysForRelevance
                ? RatingQualificationStatus.Reachable
                : RatingQualificationStatus.OutOfReach;

        return new RatingQualification(daysFlown.Count, status);
    }
}
