using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Web.Controllers.Heatmap;

public class PilotResultsCalculator
{
    private readonly IRepository<CompetitionResults> _competitionResults;

    public PilotResultsCalculator(IRepository<CompetitionResults> competitionResults)
    {
        _competitionResults = competitionResults;
    }

    public async Task<List<PilotResult>> GetPilotResults(string pilotName, CancellationToken ct)
    {
        var start = new DateTime(2024, 1, 1);

        var data = await _competitionResults
            .GetAll()
            .Where(c => c.Competition.StartedOn >= start)
            .Where(c => c.PlayerName == pilotName && c.Competition.State == CompetitionState.Closed)
            .OrderBy(x => x.Competition.StartedOn)
            .ProjectToModel()
            .ToListAsync(ct);

        return data;
    }
}
