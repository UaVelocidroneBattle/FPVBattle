using Hangfire;
using MediatR;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Services.Achievements;

public class AchievementsEventHandler :
    INotificationHandler<CurrentResultUpdateMessage>,
    INotificationHandler<CompetitionStopped>,
    INotificationHandler<SeasonFinished>
{
    private readonly AchievementService _achievementService;

    public AchievementsEventHandler(AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        BackgroundJob.Enqueue(() => _achievementService.CheckAfterTimeUpdateAsync(notification.Deltas, cancellationToken));
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        BackgroundJob.Enqueue(() => _achievementService.CheckAfterCompetitionAndGlobalsAsync(notification.Competition, cancellationToken));
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        BackgroundJob.Enqueue(() => _achievementService.CheckAfterSeasonAsync(notification.Results, cancellationToken));
    }
}
