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

    public async Task UpdatePilotsAsync(List<TrackTimeDelta> deltas)
    {
        _log.Debug("Updating pilots from {DeltaCount} deltas", deltas.Count);

        foreach (var delta in deltas)
        {
            await UpdatePilotAsync(delta);
        }
    }

    private async Task UpdatePilotAsync(TrackTimeDelta delta)
    {
        _log.Debug("Updating pilot for delta: {Delta}", delta);

        if (delta.UserId is null)
            throw new Exception("Delta must have a UserId");

        var pilot = await _pilots.FindAsync(delta.UserId);

        if (pilot is null)
        {
            _log.Debug("Pilot not found for UserId {UserId}, creating new pilot {PilotName}", delta.UserId, delta.PlayerName);

            var newPilot = new Pilot
            {
                Id = delta.UserId.Value,
                Name = delta.PlayerName,
            };

            await _pilots.AddAsync(newPilot);
            await _mediator.Publish(new NewPilot(newPilot));
            return;
        }

        if (pilot.Name != delta.PlayerName)
        {
            _log.Debug("Pilot name changed from {OldName} to {NewName} for UserId {UserId}", pilot.Name, delta.PlayerName, delta.UserId);

            var oldName = pilot.Name;
            pilot.ChangeName(delta.PlayerName);
            await _pilots.SaveChangesAsync();
            await _mediator.Publish(new PilotRenamed(oldName, delta.PlayerName));
        }
    }
}
