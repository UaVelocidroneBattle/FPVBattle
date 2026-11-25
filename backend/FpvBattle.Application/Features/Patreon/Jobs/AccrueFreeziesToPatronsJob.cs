using FpvBattle.Application.Features.Patreon.Models;
using FpvBattle.Application.Features.Patreon.Notifications;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace FpvBattle.Application.Features.Patreon.Jobs;

public class AccrueFreeziesToPatronsJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MonthlyPatreonSupportersJob>();
    private readonly IPublisher _publisher;
    private readonly IApplicationDbContext _db;
    private readonly PatreonOptions _options;

    public AccrueFreeziesToPatronsJob(IPublisher publisher, IApplicationDbContext db, IOptions<PatreonOptions> options)
    {
        _publisher = publisher;
        _db = db;
        _options = options.Value;
    }

    [DisableConcurrentExecution("MonthlyPatreonSupporters", 60)]
    public async Task Handle(CancellationToken ct)
    {
        var allActiveSupporters = await _db.PatreonSupporters
            .Paying()
            .ToListAsync(ct);

        if (!allActiveSupporters.Any())
        {
            Log.Information("No active supporters found for freezie accrual");
            return;
        }

        Log.Information("Found {Count} active supporters for freezie accrual", allActiveSupporters.Count);

        var today = DateTime.Today;
        var accruedFreezies = new List<AccruedPatronFreezies>();

        foreach (var supporter in allActiveSupporters)
        {
            var pilot = supporter.Pilot;

            if (pilot is null)
                continue;

            var freeziesToAdd = _options.MonthlyFreeziesFor(supporter.TierName);

            Log.Information("Accruing {FreeziesToAdd} freezies to pilot {PilotName} for supporting at tier {SupporterTierName}", freeziesToAdd, pilot.Name, supporter.TierName);

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

        await _db.SaveChangesAsync(ct);
        await _publisher.Publish(new MonthlyAccruedFreeziesNotification(accruedFreezies), ct);
    }
}
