using MediatR;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Patreon.Notifications;

namespace Veloci.Logic.Features.Patreon.NotificationHandlers;

public class TelegramPatreonHandler :
    INotificationHandler<NewPatreonSupporterNotification>,
    INotificationHandler<MonthlyPatreonSupportersNotification>
{
    public async Task Handle(MonthlyPatreonSupportersNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Supporters.Any())
        {
            return;
        }

        var message = $"📊 *Щомісячні підтримувачі Patreon* ({notification.Supporters.Count}):\n\n";

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

        message += "Дякуємо всім за постійну підтримку! 🙏";

        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(NewPatreonSupporterNotification notification, CancellationToken cancellationToken)
    {
        var message = $"🎉 Вітаємо нового підтримувача Patreon: *{notification.Supporter.Name}*";
        if (!string.IsNullOrEmpty(notification.Supporter.TierName))
        {
            message += $" ({notification.Supporter.TierName})";
        }

        message += "! Дякуємо за підтримку! ❤️";

        await TelegramBot.SendMessageAsync(message);
    }
}
