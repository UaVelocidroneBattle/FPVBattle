using Microsoft.EntityFrameworkCore;
using Veloci.Data.Repositories;
using Veloci.Domain;

namespace Veloci.Web.Controllers.Api.Heatmap;

public class PilotResultsCalculator
{
    private readonly IRepository<CompetitionResults> _competitionResults;

    public PilotResultsCalculator(IRepository<CompetitionResults> competitionResults)
    {
        _competitionResults = competitionResults;
    }

    public async Task<List<PilotResult>> GetPilotResults(Pilot pilot, CancellationToken ct)
    {
        var start = new DateTime(2024, 1, 1);

        var data = await _competitionResults
            .GetAll()
            .Where(c => c.Competition.StartedOn >= start)
            .Where(c => c.PilotId == pilot.Id && c.Competition.State == CompetitionState.Closed)
            .OrderBy(x => x.Competition.StartedOn)
            .ProjectToModel()
            .ToListAsync(ct);

        return data;
    }
}
