using MediatR;
using Veloci.Logic.Bot.Discord;
using Veloci.Logic.Features.Achievements.Notifications;
using Veloci.Logic.Features.Achievements.Services;

namespace Veloci.Logic.Features.Achievements.NotificationHandlers;

public class DiscordAchievementsHandler :
    INotificationHandler<DayStreakAchievements>,
    INotificationHandler<GotAchievements>
{
    private readonly DiscordAchievementMessageComposer _messageComposer;
    private readonly IDiscordGeneralMessenger _generalMessenger;

    public DiscordAchievementsHandler(
        DiscordAchievementMessageComposer messageComposer,
        IDiscordGeneralMessenger generalMessenger)
    {
        _messageComposer = messageComposer;
        _generalMessenger = generalMessenger;
    }

    public async Task Handle(DayStreakAchievements notification, CancellationToken cancellationToken)
    {
        const int delaySec = 3;

        foreach (var participation in notification.Participations.Where(p => p.CupIds.Count > 0))
        {
            var message = _messageComposer.DayStreakAchievement(participation.Pilot);
            await _generalMessenger.SendMessageAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.AchievementList(notification.Results);
        await _generalMessenger.SendMessageAsync(message);
    }
}
