using MediatR;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Achievements.Notifications;
using Veloci.Logic.Features.Achievements.Services;

namespace Veloci.Logic.Features.Achievements.NotificationHandlers;

public class TelegramAchievementsHandler :
    INotificationHandler<DayStreakAchievements>,
    INotificationHandler<GotAchievements>
{
    private readonly TelegramAchievementMessageComposer _messageComposer;

    public TelegramAchievementsHandler(TelegramAchievementMessageComposer messageComposer)
    {
        _messageComposer = messageComposer;
    }

    public async Task Handle(DayStreakAchievements notification, CancellationToken cancellationToken)
    {
        const int delaySec = 3;

        foreach (var pilot in notification.Pilots)
        {
            var message = _messageComposer.DayStreakAchievement(pilot);
            await TelegramBot.SendMessageAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.AchievementList(notification.Results);
        await TelegramBot.SendMessageAsync(message);
    }
}
