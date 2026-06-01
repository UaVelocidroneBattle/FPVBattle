using MediatR;
using Veloci.Logic.Notifications;
using Veloci.Logic.Features.Achievements.Services;

namespace Veloci.Logic.Features.Achievements.NotificationHandlers;

public class AchievementsEventHandler :
    INotificationHandler<CurrentResultUpdated>,
    INotificationHandler<CompetitionFinished>,
    INotificationHandler<SeasonFinished>
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
        await _achievementService.CheckGlobalsAsync();
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        var flatResults = notification.Results.SelectMany(l => l.Results).ToList();
        await _achievementService.CheckAfterSeasonAsync(flatResults, notification.CupId, cancellationToken);
    }
}
