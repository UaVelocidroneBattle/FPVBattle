using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Patreon.Notifications;

namespace Veloci.Logic.Features.Patreon.Commands;

public class MonthlyPatreonSupportersJob
{
    private static readonly ILogger _log = Log.ForContext<MonthlyPatreonSupportersJob>();
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
            .GetAll(s => s.Status == "active_patron")
            .ToListAsync(ct);

        if (!allActiveSupporters.Any())
        {
            _log.Information("No active supporters found for monthly notification");
            return;
        }

        _log.Information("Found {Count} active supporters for monthly notification", allActiveSupporters.Count);

        await _mediator.Publish(new MonthlyPatreonSupportersNotification(allActiveSupporters), ct);
    }
}
