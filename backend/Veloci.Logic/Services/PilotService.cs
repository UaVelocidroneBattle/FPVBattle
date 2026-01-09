using MediatR;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Services;

public class PilotService
{
    private static readonly ILogger _log = Log.ForContext<PilotService>();
    private readonly IRepository<Pilot> _pilots;
    private readonly IMediator _mediator;

    public PilotService(IRepository<Pilot> pilots, IMediator mediator)
    {
        _pilots = pilots;
        _mediator = mediator;
    }

    public async Task UpdatePilotsAsync(List<TrackTimeDelta> deltas, Dictionary<int, string> pilotNames)
    {
        _log.Debug("Updating pilots from {DeltaCount} deltas", deltas.Count);

        foreach (var delta in deltas)
        {
            await UpdatePilotAsync(delta, pilotNames);
        }
    }

    private async Task UpdatePilotAsync(TrackTimeDelta delta, Dictionary<int, string> pilotNames)
    {
        _log.Debug("Updating pilot for delta: {Delta}", delta);

        var pilotName = pilotNames[delta.PilotId];
        var pilot = await _pilots.FindAsync(delta.PilotId);

        if (pilot is null)
        {
            _log.Debug("Pilot not found for UserId {UserId}, creating new pilot {PilotName}", delta.PilotId, pilotName);

            var newPilot = new Pilot
            {
                Id = delta.PilotId,
                Name = pilotName,
            };

            await _pilots.AddAsync(newPilot);
            await _mediator.Publish(new NewPilot(newPilot));
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
}
