using MediatR;
using Serilog;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.Patreon.Notifications;
using Veloci.Logic.Features.Patreon.Services;

namespace Veloci.Logic.Features.Patreon.NotificationHandlers;

public class TelegramPatreonHandler :
    INotificationHandler<NewPatreonSupporterNotification>,
    INotificationHandler<MonthlyPatreonSupportersNotification>,
    INotificationHandler<MonthlyAccruedFreeziesNotification>
{
    private static readonly ILogger _log = Log.ForContext<TelegramPatreonHandler>();

    private readonly ITelegramMessenger _messenger;
    private readonly ICupService _cupService;

    public TelegramPatreonHandler(ITelegramMessenger messenger, ICupService cupService)
    {
        _messenger = messenger;
        _cupService = cupService;
    }

    public async Task Handle(MonthlyPatreonSupportersNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Supporters.Any())
        {
            return;
        }

        var message = $"📊 *Щомісячні патрони FPV Battle на Patreon* ({notification.Supporters.Count}):\n\n";

        var groupedByTier = notification.Supporters
            .GroupBy(s => s.TierName ?? "Невідомий рівень")
            .OrderByDescending(g => g.Average(s => s.Amount ?? 0));

        foreach (var tierGroup in groupedByTier)
        {
            message += $"*{tierGroup.Key}:*\n";
            foreach (var supporter in tierGroup.OrderBy(s => s.Name))
            {
                message += $"• {supporter.Name}\n";
            }

            message += "\n";
        }

        message += "Дякуємо всім за підтримку! 🙏\n\n" +
                   "Наш Patreon: *https://patreon.com/FPVBattle*";

        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public async Task Handle(NewPatreonSupporterNotification notification, CancellationToken cancellationToken)
    {
        var message = PatreonMessageGenerator.GenerateWelcomeMessage(
            notification.Supporter.Name,
            notification.Supporter.TierName,
            useDiscordMarkdown: false);

        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public async Task Handle(MonthlyAccruedFreeziesNotification notification, CancellationToken cancellationToken)
    {
        var message = PatreonMessageGenerator.AccruedFreeziesMessage(notification.Accrued, useDiscordMarkdown: false);
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
                    _log.Error(ex, "Failed to send Patreon message to cup {CupId}", cupId);
                }
            }
        }
    }
}
