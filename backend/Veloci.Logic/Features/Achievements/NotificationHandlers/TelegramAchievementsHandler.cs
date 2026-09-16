using MediatR;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Achievements.Notifications;
using Veloci.Logic.Features.Achievements.Services;

namespace Veloci.Logic.Features.Achievements.NotificationHandlers;

public class TelegramAchievementsHandler : INotificationHandler<GotAchievements>
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

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        foreach (var (cupId, results) in notification.Results.GroupByCup())
        {
            var message = _messageComposer.AchievementList(results);

            if (cupId is null)
            {
                await _cupMessenger.SendMessageToAllCupsAsync(message);
            }
            else
            {
                await _cupMessenger.SendMessageToCupAsync(cupId, message);
            }
        }
    }
}
