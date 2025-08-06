using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Notifications;

namespace Veloci.Logic.Features.Achievements.Jobs;

public class DayStreakMilestoneJob
{
    private static readonly ILogger _log = Log.ForContext<DayStreakMilestoneJob>();
    private readonly IRepository<Pilot> _pilots;
    private readonly IMediator _mediator;

    public DayStreakMilestoneJob(
        IRepository<Pilot> pilots,
        IMediator mediator)
    {
        _pilots = pilots;
        _mediator = mediator;
    }

    [DisableConcurrentExecution("DayStreakMilestone", 60)]
    public async Task Handle(CancellationToken ct = default)
    {
        _log.Information("Starting DayStreakMilestoneJob to check for milestone day streaks");

        var streaks = new[] { 10, 20, 50, 75, 100, 150, 200, 250, 300, 365, 500, 1000 };

        var pilots = await _pilots
            .GetAll(p => streaks.Any(s => s == p.DayStreak))
            .ToListAsync(ct);

        if (pilots.Count == 0)
        {
            _log.Debug("No pilots achieved milestone day streaks today");
            return;
        }

        _log.Information("Found {PilotCount} pilots with milestone day streaks: {PilotNames}",
            pilots.Count, string.Join(", ", pilots.Select(p => $"{p.Name} ({p.DayStreak})")));

        await _mediator.Publish(new DayStreakAchievements(pilots), ct);

        _log.Information("DayStreakMilestoneJob completed successfully");
    }
}
