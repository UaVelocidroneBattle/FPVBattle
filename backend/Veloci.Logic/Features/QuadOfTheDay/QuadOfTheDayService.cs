using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.QuadOfTheDay;

public class QuadOfTheDayService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<QuadOfTheDayService>();
    private readonly IRepository<QuadModel> _quadModels;
    private readonly IRepository<Competition> _competitions;
    private static readonly Random Random = new();

    public QuadOfTheDayService(IRepository<QuadModel> quadModels, IRepository<Competition> competitions)
    {
        _quadModels = quadModels;
        _competitions = competitions;
    }

    /// <summary>
    /// Reward strategy. Adds bonus points to those whe used quad of the day
    /// </summary>
    public void ApplyBonusPoints(Competition competition, CupOptions cupOptions)
    {
        if (competition.QuadOfTheDay is null || cupOptions.QuadOfTheDay.BonusPoints == 0)
            return;

        var eligible = competition.CompetitionResults
            .Where(r => r.ModelName == competition.QuadOfTheDay.Name)
            .ToList();

        if (eligible.Count == 0)
        {
            Log.Information("Quad-of-the-day was {QuadName} but no pilots flew it in competition {CompetitionId}",
                competition.QuadOfTheDay.Name, competition.Id);
            return;
        }

        foreach (var result in eligible)
            result.BonusPoints = cupOptions.QuadOfTheDay.BonusPoints;

        Log.Information("Applied {BonusPoints} bonus points to {PilotCount} pilots who flew quad-of-the-day {QuadName}",
            cupOptions.QuadOfTheDay.BonusPoints, eligible.Count, competition.QuadOfTheDay.Name);
    }

    /// <summary>
    /// Punishment strategy. Pilots get only 1 point if used not quad of the day
    /// </summary>
    public void PunishNonQuadOfTheDayPilots(Competition competition)
    {
        if (competition.QuadOfTheDay is null)
            return;

        var penalized = competition.CompetitionResults
            .Where(r => r.ModelName != competition.QuadOfTheDay.Name)
            .ToList();

        if (penalized.Count == 0)
        {
            Log.Information("All pilots flew quad-of-the-day {QuadName} in competition {CompetitionId}",
                competition.QuadOfTheDay.Name, competition.Id);
            return;
        }

        foreach (var result in penalized)
            result.Points = 1;

        Log.Information("Penalized {PilotCount} pilots to 1 point for not flying quad-of-the-day {QuadName}",
            penalized.Count, competition.QuadOfTheDay.Name);
    }

    public async Task<QuadModel?> DetectQuadFromTrackNameAsync(string trackName, CupOptions cupOptions)
    {
        var options = cupOptions.QuadOfTheDay;

        if (!options.Enabled || options.Quads.Length == 0)
            return null;

        var matchedName = options.Quads
            .FirstOrDefault(q => trackName.Contains(q, StringComparison.OrdinalIgnoreCase));

        if (matchedName is null)
            return null;

        Log.Information("Detected quad {QuadName} from track name '{TrackName}'", matchedName, trackName);

        var quad = await _quadModels
            .GetAll()
            .FirstOrDefaultAsync(q => q.Name == matchedName);

        if (quad is null)
            Log.Warning("Quad {QuadName} detected from track name but not found in the database", matchedName);

        return quad;
    }

    public async Task<QuadModel?> GetQuadOfTheDayAsync(CupOptions cupOptions, string cupId)
    {
        if (await LastCompetitionWasQuadOfTheDayAsync(cupId))
            return null;

        var options = cupOptions.QuadOfTheDay;

        if (!options.Enabled || options.Quads.Length == 0)
            return null;

        var triggered = Random.Next(100) < options.Probability;

        if (!triggered)
        {
            Log.Debug("Quad-of-the-day not triggered (probability: {Probability}%)", options.Probability);
            return null;
        }

        var quadName = options.Quads[Random.Next(options.Quads.Length)];
        Log.Information("Quad-of-the-day triggered: selected {QuadName}", quadName);

        var quad = await _quadModels
            .GetAll()
            .FirstOrDefaultAsync(q => q.Name == quadName);

        if (quad is null)
            Log.Warning("Quad-of-the-day {QuadName} not found in the database", quadName);

        return quad;
    }

    private async Task<bool> LastCompetitionWasQuadOfTheDayAsync(string cupId)
    {
        var lastCompetition = await _competitions
            .GetAll(c => c.State == CompetitionState.Closed)
            .ForCup(cupId)
            .OrderByDescending(c => c.StartedOn)
            .FirstOrDefaultAsync();

        return lastCompetition?.QuadOfTheDay != null;
    }
}
