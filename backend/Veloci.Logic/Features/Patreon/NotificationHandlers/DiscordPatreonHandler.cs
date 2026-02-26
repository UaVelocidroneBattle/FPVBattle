using MediatR;
using Veloci.Logic.Bot.Discord;
using Veloci.Logic.Features.Patreon.Notifications;
using Veloci.Logic.Features.Patreon.Services;

namespace Veloci.Logic.Features.Patreon.NotificationHandlers;

public class DiscordPatreonHandler :
    INotificationHandler<NewPatreonSupporterNotification>,
    INotificationHandler<MonthlyPatreonSupportersNotification>,
    INotificationHandler<MonthlyAccruedFreeziesNotification>
{
    private readonly IDiscordCupMessenger _discordCupMessenger;

    public DiscordPatreonHandler(IDiscordCupMessenger discordCupMessenger)
    {
        _discordCupMessenger = discordCupMessenger;
    }

    public async Task Handle(MonthlyPatreonSupportersNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Supporters.Any())
        {
            return;
        }

        var message = $"📊 **FPV Battle Patreon supporters** ({notification.Supporters.Count}):\n\n";

        var groupedByTier = notification.Supporters
            .GroupBy(s => s.TierName ?? "Unknown tier")
            .OrderByDescending(g => g.Average(s => s.Amount ?? 0));

        foreach (var tierGroup in groupedByTier)
        {
            message += $"**{tierGroup.Key}:**\n";
            foreach (var supporter in tierGroup.OrderBy(s => s.Name))
            {
                message += $"•  {supporter.Name}\n";
            }

            message += "\n";
        }

        message += "Thank you all for your support! 🙏\n\n" +
                   "👉 [Our Patreon](https://patreon.com/FPVBattle)";

        await _discordCupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(NewPatreonSupporterNotification notification, CancellationToken cancellationToken)
    {
        var message = PatreonMessageGenerator.GenerateWelcomeMessage(
            notification.Supporter.Name,
            notification.Supporter.TierName,
            useDiscordMarkdown: true);

        await _discordCupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(MonthlyAccruedFreeziesNotification notification, CancellationToken cancellationToken)
    {
        var message = PatreonMessageGenerator.AccruedFreeziesMessage(notification.Accrued, useDiscordMarkdown: true);
        await _discordCupMessenger.SendMessageToAllCupsAsync(message);
    }
}
