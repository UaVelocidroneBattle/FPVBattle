using MediatR;
using Veloci.Logic.Notifications;
using Veloci.Logic.Features.Achievements.Services;

namespace Veloci.Logic.Features.Achievements.NotificationHandlers;

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
        await _achievementService.CheckAfterTimeUpdateAsync(notification.Competition, notification.Deltas, cancellationToken);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        await _achievementService.CheckAfterCompetitionAsync(notification.Competition, cancellationToken);
        await _achievementService.CheckGlobalsAsync();
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        await _achievementService.CheckAfterSeasonAsync(notification.Results, notification.CupId, cancellationToken);
    }
}
