using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Logic.Features.PilotBinding.Services;
using FpvBattle.Logic.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Logic.Features.PilotBinding.NotificationHandlers;

/// <summary>
/// Watches daily results and completes pending pilot claims: a claimed pilot
/// posting a time on the current track is the "fly-to-verify" proof.
/// </summary>
public class PilotClaimResolutionHandler : INotificationHandler<CurrentResultUpdated>
{
    private readonly PilotBindingService _bindingService;
    private readonly IApplicationDbContext _db;

    public PilotClaimResolutionHandler(PilotBindingService bindingService, IApplicationDbContext db)
    {
        _bindingService = bindingService;
        _db = db;
    }

    public async Task Handle(CurrentResultUpdated notification, CancellationToken cancellationToken)
    {
        var pilotIds = notification.Deltas
            .Select(d => d.PilotId)
            .Distinct()
            .ToList();

        var racedPilots = await _db.Pilots.Where(p => pilotIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        await _bindingService.CompleteClaimsAsync(racedPilots, cancellationToken);
    }
}
