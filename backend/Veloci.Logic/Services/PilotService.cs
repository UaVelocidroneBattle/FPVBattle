using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Services;

public class PilotService
{
    private static readonly ILogger _log = Log.ForContext<PilotService>();
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<TrackTimeDelta> _deltas;
    private readonly IMediator _mediator;

    public PilotService(
        IRepository<Pilot> pilots,
        IMediator mediator,
        IRepository<TrackTimeDelta> deltas)
    {
        _pilots = pilots;
        _mediator = mediator;
        _deltas = deltas;
    }

    public async Task UpdatePilotsAsync(List<TrackTimeDelta> deltas, Dictionary<int, string> pilotNames, string cupId)
    {
        _log.Debug("Updating pilots from {DeltaCount} deltas for cup {CupId}", deltas.Count, cupId);

        foreach (var delta in deltas)
        {
            await UpdatePilotAsync(delta, pilotNames, cupId);
        }
    }

    private async Task UpdatePilotAsync(TrackTimeDelta delta, Dictionary<int, string> pilotNames, string cupId)
    {
        _log.Debug("Updating pilot for delta: {Delta} in cup {CupId}", delta, cupId);

        var pilotName = pilotNames[delta.PilotId];
        var pilot = await _pilots.FindAsync(delta.PilotId);

        if (pilot is null)
        {
            _log.Debug("Pilot not found for UserId {UserId}, creating new pilot {PilotName} in cup {CupId}", delta.PilotId, pilotName, cupId);

            var newPilot = new Pilot
            {
                Id = delta.PilotId,
                Name = pilotName,
            };

            await _pilots.AddAsync(newPilot);
            await _mediator.Publish(new NewPilot(newPilot, cupId));
            return;
        }

        if (pilot.Name != pilotName)
        {
            _log.Debug("Pilot name changed from {OldName} to {NewName} for UserId {UserId}", pilot.Name, pilotName, delta.PilotId);

            var oldName = pilot.Name;
            pilot.ChangeName(pilotName);
            await _pilots.SaveChangesAsync();
            await _mediator.Publish(new PilotRenamed(oldName, pilotName));
        }
    }

    public async Task DayStreakPotentialLoseNotificationAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var participatedPilotIds = await _deltas
            .GetAll()
            .Where(d => d.Date >= today && d.Date < tomorrow)
            .Select(d => d.PilotId)
            .Distinct()
            .ToListAsync();

        var pilotsToNotify = await _pilots
            .GetAll(p => p.DayStreak > 10)
            .Where(p => participatedPilotIds.All(l => l != p.Id))
            .ToListAsync();

        if (pilotsToNotify.Count == 0)
        {
            _log.Debug("All pilots with significant day streaks have already participated today");
            return;
        }

        _log.Information("Found {PilotCount} pilots at risk of losing day streaks: {PilotNames}",
            pilotsToNotify.Count, string.Join(", ", pilotsToNotify.Select(p => $"{p.Name} ({p.DayStreak})")));

        await _mediator.Publish(new DayStreakPotentialLose(pilotsToNotify));
    }

    public async Task UpdatePilotDayStreaksAsync()
    {
        _log.Debug("Updating pilot day streaks");

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        var pilots = await _deltas.GetAll()
            .Include(d => d.Pilot)
            .ThenInclude(p => p.DayStreakFreezes)
            .Where(d => d.Date >= yesterday && d.Date < today)
            .Select(d => d.Pilot)
            .Distinct()
            .ToListAsync();

        foreach (var pilot in pilots)
        {
            pilot.OnRaceFlown(yesterday);
        }

        await _pilots.SaveChangesAsync();
        await _pilots.GetAll().ResetDayStreaksAsync(yesterday);
        await _pilots.SaveChangesAsync();

        _log.Information("Updated day streaks: {ActiveCount} pilots flew, streaks reset for inactive pilots", pilots.Count);
    }
}
