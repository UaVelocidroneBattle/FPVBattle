using FpvBattle.Application.Bot.Telegram;
using FpvBattle.Application.Features.Patreon.Notifications;
using FpvBattle.Application.Features.Patreon.Services;
using MediatR;

namespace FpvBattle.Application.Features.Patreon.NotificationHandlers;

public class TelegramPatreonHandler :
    INotificationHandler<NewPatreonSupportersNotification>,
    INotificationHandler<MonthlyPatreonSupportersNotification>,
    INotificationHandler<MonthlyAccruedFreeziesNotification>
{
    private readonly ITelegramCupMessenger _cupMessenger;

    public TelegramPatreonHandler(ITelegramCupMessenger cupMessenger)
    {
        _cupMessenger = cupMessenger;
    }

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

        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(NewPatreonSupportersNotification notification, CancellationToken cancellationToken)
    {
        var message = TelegramMessageGenerator.WelcomeMessage(notification.Supporters);

        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(MonthlyAccruedFreeziesNotification notification, CancellationToken cancellationToken)
    {
        var message = TelegramMessageGenerator.AccruedFreeziesMessage(notification.Accrued);
        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }
}
