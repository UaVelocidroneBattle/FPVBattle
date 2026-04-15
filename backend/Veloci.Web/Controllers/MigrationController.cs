using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using ILogger = Serilog.ILogger;

namespace Veloci.Web.Controllers;

[ApiController]
[Route("/api/migration/[action]")]
public class MigrationController : ControllerBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MigrationController>();
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<CompetitionResults> _competitionResults;

    public MigrationController(IRepository<Pilot> pilots, IRepository<CompetitionResults> competitionResults)
    {
        _pilots = pilots;
        _competitionResults = competitionResults;
    }

    [HttpGet]
    public async Task CalculatePilotProperties()
    {
        var pilots = await _pilots.GetAll().ToListAsync();
        Log.Information("Starting pilot properties migration for {PilotCount} pilots", pilots.Count);

        var updated = 0;
        var skipped = 0;

        foreach (var pilot in pilots)
        {
            var raceDates = _competitionResults.GetAll()
                .Where(cr => cr.PilotId == pilot.Id && cr.Competition.State == CompetitionState.Closed)
                .Select(cr => cr.Competition.StartedOn.Date)
                .Distinct();

            if (!await raceDates.AnyAsync())
            {
                Log.Warning("Pilot {PilotName} has no race dates. Skipping", pilot.Name);
                skipped++;
                continue;
            }

            pilot.CreatedAt = await raceDates.MinAsync();
            pilot.TotalRaceDays = await raceDates.CountAsync();

            Log.Debug("Updated pilot {PilotName}: CreatedAt={CreatedAt}, TotalRaceDays={TotalRaceDays}",
                pilot.Name, pilot.CreatedAt, pilot.TotalRaceDays);
            updated++;
        }

        await _pilots.SaveChangesAsync();
        Log.Information("Pilot properties migration complete. Updated: {Updated}, Skipped: {Skipped}", updated, skipped);
    }
}
