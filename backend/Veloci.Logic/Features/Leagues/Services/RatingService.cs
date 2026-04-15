using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Leagues.Services;

public class RatingService
{
    private readonly IRepository<PilotPaceRating> _ratings;

    public RatingService(IRepository<PilotPaceRating> ratings)
    {
        _ratings = ratings;
    }

    public async Task<IList<PilotPaceRating>> GetRatingsForCupAsync(string cupId)
    {
        var lastDate = await GetLastCalculationDateAsync(cupId);

        if (lastDate is null)
            return [];

        return await _ratings
            .GetAll(r => r.CupId == cupId && r.CalculatedOn == lastDate.Value)
            .OrderBy(r => r.Rank)
            .Include(r => r.Pilot)
            .ToListAsync();
    }

    public async Task<int?> GetPilotRankAsync(string cupId, int pilotId)
    {
        var lastDate = await GetLastCalculationDateAsync(cupId);

        if (lastDate is null)
            return null;

        var pilotRank = await _ratings
            .GetAll(r => r.CupId == cupId && r.CalculatedOn == lastDate.Value)
            .FirstOrDefaultAsync(r => r.PilotId == pilotId);

        return pilotRank?.Rank;
    }

    private async Task<DateTime?> GetLastCalculationDateAsync(string cupId)
    {
        return await _ratings
            .GetAll(r => r.CupId == cupId)
            .OrderByDescending(r => r.CalculatedOn)
            .Select(r => (DateTime?)r.CalculatedOn)
            .FirstOrDefaultAsync();
    }
}
