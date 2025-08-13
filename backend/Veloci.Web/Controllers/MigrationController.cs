using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Web.Controllers;

[ApiController]
[Route("/api/migration/[action]")]
public class MigrationController
{
    private readonly IRepository<Competition> _competitions;
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<TrackTimeDelta> _trackTimeDeltas;
    private readonly IRepository<CompetitionResults> _competitionResults;

    public MigrationController(
        IRepository<Competition> competitions,
        IRepository<Pilot> pilots,
        IRepository<TrackTimeDelta> trackTimeDeltas,
        IRepository<CompetitionResults> competitionResults)
    {
        _competitions = competitions;
        _pilots = pilots;
        _trackTimeDeltas = trackTimeDeltas;
        _competitionResults = competitionResults;
    }

    [HttpGet("/api/migration/streaks")]
    public async Task Streaks()
    {
        var competitions = await _competitions
            .GetAll()
            .OrderBy(c => c.StartedOn)
            .Where(c => c.State == CompetitionState.Closed)
            .ToListAsync();

        var pilotList = new List<Pilot>();

        foreach (var comp in competitions)
        {
            ProcessCompetition(comp, pilotList);
        }

        foreach (var pilotToUpdate in pilotList)
        {
            await UpdatePilotAsync(pilotToUpdate);
        }

        await _pilots.SaveChangesAsync();
    }

    private void ProcessCompetition(Competition comp, List<Pilot> pilotList)
    {
        var today = comp.StartedOn.AddDays(1);

        var pilotIds = comp.CompetitionResults
            .Where(p => p.UserId != null)
            .Select(x => x.UserId.Value)
            .ToList();

        var pilotsSkippedDay = pilotList
            .Where(p => !pilotIds.Contains(p.Id))
            .ToList();

        foreach (var pilot in pilotsSkippedDay)
        {
            pilot.ResetDayStreak(today);
        }

        foreach (var pilotId in pilotIds)
        {
            var listed = pilotList.FirstOrDefault(x => x.Id == pilotId);

            if (listed is null)
            {
                pilotList.Add(new Pilot
                {
                    Id = pilotId,
                    DayStreak = 1,
                    DayStreakFreezes = new List<DayStreakFreeze>()
                });
            }
            else
            {
                listed.OnRaceFlown(today);
            }
        }
    }

    private async Task UpdatePilotAsync(Pilot pilotToUpdate)
    {
        var pilot = await _pilots.FindAsync(pilotToUpdate.Id);
        pilot.DayStreak = pilotToUpdate.DayStreak;
        pilot.MaxDayStreak = pilotToUpdate.MaxDayStreak;
        pilot.DayStreakFreezes.Clear();

        foreach (var freezie in pilotToUpdate.DayStreakFreezes)
        {
            pilot.DayStreakFreezes.Add(new DayStreakFreeze(freezie.CreatedOn)
            {
                SpentOn = freezie.SpentOn
            });
        }
    }

    [HttpGet("/api/migration/pilot-ids")]
    public async Task SetPilotIdsToEntities()
    {
        var pilots = await _pilots.GetAll().ToListAsync();

        foreach (var pilot in pilots)
        {
            await SetPilotIdToEntitiesAsync(pilot);
        }

        await _trackTimeDeltas.SaveChangesAsync();
        Log.Debug("Finished setting pilot IDs to entities");
    }

    private string[] ExtractPilotNames(Pilot pilot)
    {
        var names = new List<string> { pilot.Name };

        var oldNames = pilot.NameHistory?
            .Select(n => n.OldName)
            .ToList();

        if (oldNames != null && oldNames.Count != 0)
            names.AddRange(oldNames);

        return names.ToArray();
    }


    private async Task SetPilotIdToEntitiesAsync(Pilot pilot)
    {
        Log.Debug("Setting pilot ID {PilotId} to entities for pilot {PilotName}", pilot.Id, pilot.Name);

        var pilotNames = ExtractPilotNames(pilot);

        var deltas = await _trackTimeDeltas
            .GetAll()
            .Where(d => pilotNames.Contains(d.PlayerName))
            .ToListAsync();

        foreach (var delta in deltas)
        {
            delta.UserId = pilot.Id;
        }

        var compResults = await _competitionResults
            .GetAll()
            .Where(r => pilotNames.Contains(r.PlayerName))
            .ToListAsync();

        foreach (var result in compResults)
        {
            result.UserId = pilot.Id;
        }
    }
}
