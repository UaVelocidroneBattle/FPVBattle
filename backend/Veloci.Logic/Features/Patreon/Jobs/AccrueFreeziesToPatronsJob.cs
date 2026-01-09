using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Patreon.Models;
using Veloci.Logic.Features.Patreon.Notifications;

namespace Veloci.Logic.Features.Patreon.Jobs;

public class AccrueFreeziesToPatronsJob
{
    private static readonly ILogger _log = Log.ForContext<MonthlyPatreonSupportersJob>();
    private readonly IMediator _mediator;
    private readonly IRepository<PatreonSupporter> _supportersRepository;

    public AccrueFreeziesToPatronsJob(IMediator mediator, IRepository<PatreonSupporter> supportersRepository)
    {
        _mediator = mediator;
        _supportersRepository = supportersRepository;
    }

    [DisableConcurrentExecution("MonthlyPatreonSupporters", 60)]
    public async Task Handle(CancellationToken ct)
    {
        var allActiveSupporters = await _supportersRepository
            .GetAll(s => s.Status == "active_patron")
            .ToListAsync(ct);

        if (!allActiveSupporters.Any())
        {
            _log.Information("No active supporters found for freezie accrual");
            return;
        }

        _log.Information("Found {Count} active supporters for freezie accrual", allActiveSupporters.Count);

        var today = DateTime.Today;
        var accruedFreezies = new List<AccruedPatronFreezies>();

        foreach (var supporter in allActiveSupporters)
        {
            var pilot = supporter.Pilot;

            if (pilot is null)
                continue;

            var freeziesToAdd = CalculateFreeziesForSupporter(supporter);

            _log.Information("Accruing {FreeziesToAdd} freezies to pilot {PilotName} for supporting at tier {SupporterTierName}", freeziesToAdd, pilot.Name, supporter.TierName);

            for (var i = 0; i < freeziesToAdd; i++)
            {
                pilot.DayStreakFreezes.Add(new DayStreakFreeze(today));
            }

            accruedFreezies.Add(new AccruedPatronFreezies
            {
                PilotName = pilot.Name,
                FreeziesAccrued = freeziesToAdd,
            });
        }

        if (accruedFreezies.Count == 0)
            return;

        await _supportersRepository.SaveChangesAsync(ct);
        await _mediator.Publish(new MonthlyAccruedFreeziesNotification(accruedFreezies), ct);
    }

    private int CalculateFreeziesForSupporter(PatreonSupporter supporter)
    {
        return supporter.TierName switch
        {
            "Level 1" => 1,
            "Level 2" => 3,
            "Level 3" => 5,
            "Level 4" => 10,
            "Level 5" => 20,
            _ => 0
        };
    }
}
