using MediatR;
using Serilog;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Achievements.Notifications;
using Veloci.Logic.Features.Achievements.Services;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.NotificationHandlers;

public class TelegramAchievementsHandler :
    INotificationHandler<DayStreakAchievements>,
    INotificationHandler<GotAchievements>
{
    private static readonly ILogger _log = Log.ForContext<TelegramAchievementsHandler>();

    private readonly TelegramAchievementMessageComposer _messageComposer;
    private readonly ITelegramMessenger _messenger;
    private readonly ICupService _cupService;

    public TelegramAchievementsHandler(
        TelegramAchievementMessageComposer messageComposer,
        ITelegramMessenger messenger,
        ICupService cupService)
    {
        _messageComposer = messageComposer;
        _messenger = messenger;
        _cupService = cupService;
    }

    public async Task Handle(DayStreakAchievements notification, CancellationToken cancellationToken)
    {
        const int delaySec = 3;

        foreach (var pilot in notification.Pilots)
        {
            var message = _messageComposer.DayStreakAchievement(pilot);
            await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.AchievementList(notification.Results);
        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    /// <summary>
    /// Sends a message to all enabled cups that have Telegram configured
    /// </summary>
    private async Task SendToAllCupsAsync(Func<string, Task> sendAction)
    {
        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();

        foreach (var cupId in enabledCupIds)
        {
            var channelId = _cupService.GetTelegramChannelId(cupId);
            if (!string.IsNullOrEmpty(channelId))
            {
                try
                {
                    await sendAction(channelId);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Failed to send achievement message to cup {CupId}", cupId);
                }
            }
        }
    }
}
