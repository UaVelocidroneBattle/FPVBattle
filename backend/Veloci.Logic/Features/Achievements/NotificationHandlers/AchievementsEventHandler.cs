using MediatR;
using Veloci.Logic.Notifications;
using Veloci.Logic.Features.Achievements.Services;
using Veloci.Logic.Features.Leagues.Notifications;

namespace Veloci.Logic.Features.Achievements.NotificationHandlers;

public class AchievementsEventHandler :
    INotificationHandler<CurrentResultUpdated>,
    INotificationHandler<CompetitionFinished>,
    INotificationHandler<SeasonFinished>,
    INotificationHandler<LeagueUpdateNotification>
{
    private readonly AchievementService _achievementService;

    public AchievementsEventHandler(AchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task Handle(CurrentResultUpdated notification, CancellationToken cancellationToken)
    {
        await _achievementService.CheckAfterTimeUpdateAsync(notification.Competition, notification.Deltas, cancellationToken);
    }

    public async Task Handle(CompetitionFinished notification, CancellationToken cancellationToken)
    {
        await _achievementService.CheckAfterCompetitionAsync(notification.Competition, cancellationToken);
        await _achievementService.CheckGlobalsAsync(cancellationToken);
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        await _achievementService.CheckAfterSeasonAsync(notification.Results,notification.CupId, cancellationToken);
    }

    public async Task Handle(LeagueUpdateNotification notification, CancellationToken cancellationToken)
    {
        await _achievementService.CheckAfterLeagueUpdateAsync(notification.CupId, notification.Updates, cancellationToken);
    }
}
