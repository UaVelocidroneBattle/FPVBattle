using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API;

namespace Veloci.Logic.Services;

// Remove after work is done
public class PilotIdGrabber
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PilotIdGrabber>();

    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<Competition> _competitions;
    private readonly Velocidrone _velocidrone;
    private readonly RaceResultsConverter _resultsConverter;

    public PilotIdGrabber(
        IRepository<Pilot> pilots,
        IRepository<Competition> competitions,
        Velocidrone velocidrone,
        RaceResultsConverter resultsConverter)
    {
        _pilots = pilots;
        _competitions = competitions;
        _velocidrone = velocidrone;
        _resultsConverter = resultsConverter;
    }

    public async Task GrabPilotIds()
    {
        Log.Debug("Starting to grab pilot IDs");

        var pilotQuery = _pilots.GetAll(p => p.Id == null);
        var count = await pilotQuery.CountAsync();

        Log.Information("Found {Count} pilots without IDs", count);

        if (count == 0)
        {
            Log.Information("All pilots already have IDs");
            return;
        }

        var rnd = new Random();
        var randomIndex = rnd.Next(count);
        var pilot = pilotQuery.ElementAt(randomIndex);

        Log.Information("Found pilot without ID: {PilotName}", pilot.Name);

        var competition = await _competitions.GetAll()
            .OrderByDescending(c => c.StartedOn)
            .FirstOrDefaultAsync(c => c.CompetitionResults.Any(cr => cr.PlayerName == pilot.Name));

        if (competition is null)
        {
            Log.Error("Could not find any competition with pilot {Name} in results", pilot.Name);
            return;
        }

        var resultsDto = await _velocidrone.LeaderboardAsync(competition.Track.TrackId);
        var times = _resultsConverter.ConvertTrackTimes(resultsDto);

        Log.Information("Trying to find and assign pilot ID for {Count} grabbed results", times.Count);

        foreach (var time in times)
        {
            await FindAndAssignPilotIdAsync(time);
        }

        Log.Information("Finished processing results. Saving changes.");
        await _pilots.SaveChangesAsync();
    }

    private async Task FindAndAssignPilotIdAsync(TrackTime time)
    {
        var pilot = await _pilots.FindAsync(time.PlayerName);

        if (pilot is null)
        {
            Log.Warning("Pilot {PlayerName} not found in database, skipping", time.PlayerName);
            return;
        }

        if (pilot.Id is not null)
        {
            Log.Information("Pilot {PlayerName} already has an ID {PilotId}, skipping", pilot.Name, pilot.Id);
            return;
        }

        Log.Information("Found pilot {PlayerName}. Assigning ID {PilotId}", pilot.Name, time.UserId);
        pilot.Id = time.UserId;
    }
}
