using FpvBattle.Application.Features.Patreon.Notifications;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Features.Patreon.Jobs;

public class MonthlyPatreonSupportersJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MonthlyPatreonSupportersJob>();
    private readonly IPublisher _publisher;
    private readonly IApplicationDbContext _db;

    public MonthlyPatreonSupportersJob(
        IApplicationDbContext db,
        IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    [DisableConcurrentExecution("MonthlyPatreonSupporters", 60)]
    public async Task Handle(CancellationToken ct)
    {
        var allActiveSupporters = await _db.PatreonSupporters
            .Paying()
            .ToListAsync(ct);

        if (!allActiveSupporters.Any())
        {
            Log.Information("No active supporters found for monthly notification");
            return;
        }

        Log.Information("Found {Count} active supporters for monthly notification", allActiveSupporters.Count);

        await _publisher.Publish(new MonthlyPatreonSupportersNotification(allActiveSupporters), ct);
    }
}
