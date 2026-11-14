using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Web.Controllers.Api.Heatmap;

public class PilotResultsCalculator
{
    private readonly IApplicationDbContext _db;

    public PilotResultsCalculator(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<PilotResult>> GetPilotResults(Pilot pilot, CancellationToken ct)
    {
        var start = new DateTime(2024, 1, 1);

        var data = await _db.CompetitionResults
            .Where(c => c.Competition.StartedOn >= start)
            .Where(c => c.PilotId == pilot.Id && c.Competition.State == CompetitionState.Closed)
            .OrderBy(x => x.Competition.StartedOn)
            .ProjectToModel()
            .ToListAsync(ct);

        return data;
    }
}
