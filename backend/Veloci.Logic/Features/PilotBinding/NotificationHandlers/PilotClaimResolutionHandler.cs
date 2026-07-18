using MediatR;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.PilotBinding.Services;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Features.PilotBinding.NotificationHandlers;

/// <summary>
/// Watches daily results and completes pending pilot claims: a claimed pilot
/// posting a time on the current track is the "fly-to-verify" proof.
/// </summary>
public class PilotClaimResolutionHandler : INotificationHandler<CurrentResultUpdated>
{
    private readonly PilotBindingService _bindingService;
    private readonly IRepository<Pilot> _pilots;

    public PilotClaimResolutionHandler(PilotBindingService bindingService, IRepository<Pilot> pilots)
    {
        _bindingService = bindingService;
        _pilots = pilots;
    }

    public async Task Handle(CurrentResultUpdated notification, CancellationToken cancellationToken)
    {
        var pilotIds = notification.Deltas
            .Select(d => d.PilotId)
            .Distinct()
            .ToList();

        var racedPilots = await _pilots
            .GetAll(p => pilotIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        await _bindingService.CompleteClaimsAsync(racedPilots, cancellationToken);
    }
}
