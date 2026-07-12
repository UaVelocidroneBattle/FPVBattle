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
