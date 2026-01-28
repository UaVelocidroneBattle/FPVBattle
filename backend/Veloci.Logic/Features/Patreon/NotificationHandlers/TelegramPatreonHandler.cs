using MediatR;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Patreon.Notifications;
using Veloci.Logic.Features.Patreon.Services;

namespace Veloci.Logic.Features.Patreon.NotificationHandlers;

public class TelegramPatreonHandler :
    INotificationHandler<NewPatreonSupporterNotification>,
    INotificationHandler<MonthlyPatreonSupportersNotification>,
    INotificationHandler<MonthlyAccruedFreeziesNotification>
{
    public async Task Handle(MonthlyPatreonSupportersNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Supporters.Any())
        {
            return;
        }

        var message = $"📊 *Патрони FPV Battle на Patreon* ({notification.Supporters.Count}):\n\n";

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

        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(NewPatreonSupporterNotification notification, CancellationToken cancellationToken)
    {
        var message = PatreonMessageGenerator.GenerateWelcomeMessage(
            notification.Supporter.Name,
            notification.Supporter.TierName,
            useDiscordMarkdown: false);

        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(MonthlyAccruedFreeziesNotification notification, CancellationToken cancellationToken)
    {
        var message = PatreonMessageGenerator.AccruedFreeziesMessage(notification.Accrued, useDiscordMarkdown: false);
        await TelegramBot.SendMessageAsync(message);
    }
}
