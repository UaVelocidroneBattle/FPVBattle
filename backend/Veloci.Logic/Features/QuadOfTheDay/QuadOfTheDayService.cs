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
    private static readonly Random Random = new();

    public QuadOfTheDayService(IRepository<QuadModel> quadModels)
    {
        _quadModels = quadModels;
    }

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

    public async Task<QuadModel?> GetQuadOfTheDayAsync(CupOptions cupOptions)
    {
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
}
