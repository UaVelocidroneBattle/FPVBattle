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
    private readonly ITelegramCupMessenger _cupMessenger;

    public TelegramAchievementsHandler(
        TelegramAchievementMessageComposer messageComposer,
        ITelegramCupMessenger cupMessenger)
    {
        _messageComposer = messageComposer;
        _cupMessenger = cupMessenger;
    }

    public async Task Handle(DayStreakAchievements notification, CancellationToken cancellationToken)
    {
        const int delaySec = 3;

        foreach (var participation in notification.Participations.Where(p => p.CupIds.Count > 0))
        {
            var message = _messageComposer.DayStreakAchievement(participation.Pilot);
            await _cupMessenger.SendMessageToCupsAsync(participation.CupIds, message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        foreach (var (cupId, results) in notification.Results.GroupByCup())
        {
            var message = _messageComposer.AchievementList(results);
            await _cupMessenger.SendMessageToCupAsync(cupId, message);
        }
    }
}
