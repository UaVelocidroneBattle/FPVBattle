using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Leagues.Services;

public class PaceRatingCalculator
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PaceRatingCalculator>();

    private readonly IRepository<Competition> _competitions;
    private readonly IRepository<PilotPaceRating> _ratings;
    private readonly ICupService _cupService;
    private readonly RatingService _ratingService;
    private readonly PaceRatingSettings _settings;

    public PaceRatingCalculator(
        IRepository<Competition> competitions,
        ICupService cupService,
        IRepository<PilotPaceRating> ratings,
        RatingService ratingService,
        IOptions<PaceRatingSettings> settings)
    {
        _competitions = competitions;
        _cupService = cupService;
        _ratings = ratings;
        _ratingService = ratingService;
        _settings = settings.Value;
    }

    public async Task CalculateAsync()
    {
        var cupIds = _cupService.GetEnabledCupIds().ToList();

        Log.Information("Starting pace rating calculation for {CupCount} cups", cupIds.Count);

        foreach (var cupId in cupIds)
        {
            try
            {
                await CalculateForCupAsync(cupId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to calculate pace ratings for cup {CupId}", cupId);
            }
        }
    }

    private async Task CalculateForCupAsync(string cupId)
    {
        var today = DateTime.Today;
        var since = today.AddDays(-_settings.LookBackDays);

        var alreadyCalculatedToday = await _ratings.GetAll()
            .AnyAsync(r => r.CupId == cupId && r.CalculatedOn == today);

        if (alreadyCalculatedToday)
        {
            Log.Information("Cup {CupId}: pace ratings already calculated today, skipping", cupId);
            return;
        }

        var previousRatings = await _ratingService.GetRatingsForCupAsync(cupId);

        var competitions = await _competitions.GetAll()
            .ForCup(cupId)
            .Where(c => c.StartedOn >= since)
            .Where(c => c.State == CompetitionState.Closed)
            .Include(c => c.CompetitionResults)
            .ToListAsync();

        Log.Information("Cup {CupId}: found {CompetitionCount} competitions in the last {Days} days",
            cupId, competitions.Count, _settings.LookBackDays);

        var ratings = competitions
            .SelectMany(ComputeCompetitionStats)
            .GroupBy(s => s.PilotId)
            .Where(g => g.Count() >= _settings.MinDaysForRelevance)
            .Select(g => new PilotPaceRating
            {
                PilotId = g.Key,
                CupId = cupId,
                AverageGapPercent = g.Average(x => x.Gap),
                CalculatedOn = today
            })
            .OrderBy(r => r.AverageGapPercent)
            .ToList();

        for (var i = 0; i < ratings.Count; i++)
            ratings[i].Rank = i + 1;

        foreach (var rating in ratings)
        {
            var prev = previousRatings.FirstOrDefault(p => p.PilotId == rating.PilotId);

            if (prev is null)
                continue;

            rating.AverageGapChange = rating.AverageGapPercent - prev.AverageGapPercent;
            rating.RankChange = rating.Rank - prev.Rank;
        }

        Log.Information("Cup {CupId}: storing {RatingCount} pilot ratings", cupId, ratings.Count);

        await _ratings.AddRangeAsync(ratings);
    }

    private IEnumerable<PilotStats> ComputeCompetitionStats(Competition competition)
    {
        var referenceTime = GetTopPilotsAverageTime(competition.CompetitionResults);

        if (referenceTime is null)
            return [];

        return competition.CompetitionResults
            .Select(r => new PilotStats(r.PilotId, GapPercent(r.TrackTime, referenceTime.Value)));
    }

    private double? GetTopPilotsAverageTime(List<CompetitionResults> results)
    {
        var topTimes = results
            .OrderBy(r => r.LocalRank)
            .Take(_settings.TopPilotsForReference)
            .Select(r => r.TrackTime)
            .ToList();

        return topTimes.Count == 0
            ? null
            : topTimes.Average();
    }

    private static double GapPercent(int pilotTime, double referenceTime)
        => (pilotTime - referenceTime) / referenceTime * 100.0;

    private record PilotStats(int PilotId, double Gap);
}
