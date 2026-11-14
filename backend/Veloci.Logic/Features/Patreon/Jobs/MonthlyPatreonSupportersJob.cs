using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Patreon.Notifications;

namespace Veloci.Logic.Features.Patreon.Jobs;

public class MonthlyPatreonSupportersJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MonthlyPatreonSupportersJob>();
    private readonly IMediator _mediator;
    private readonly IRepository<PatreonSupporter> _supportersRepository;

    public MonthlyPatreonSupportersJob(
        IRepository<PatreonSupporter> supportersRepository,
        IMediator mediator)
    {
        _supportersRepository = supportersRepository;
        _mediator = mediator;
    }

    [DisableConcurrentExecution("MonthlyPatreonSupporters", 60)]
    public async Task Handle(CancellationToken ct)
    {
        var allActiveSupporters = await _supportersRepository
            .GetAll()
            .Paying()
            .ToListAsync(ct);

        if (!allActiveSupporters.Any())
        {
            Log.Information("No active supporters found for monthly notification");
            return;
        }

        Log.Information("Found {Count} active supporters for monthly notification", allActiveSupporters.Count);

        await _mediator.Publish(new MonthlyPatreonSupportersNotification(allActiveSupporters), ct);
    }
}
