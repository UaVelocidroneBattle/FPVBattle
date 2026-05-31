using MediatR;
using Veloci.Logic.Bot.Discord;
using Veloci.Logic.Features.Leagues.Notifications;

namespace Veloci.Logic.Features.Leagues.NotificationHandlers;

public class DiscordLeagueHandler : INotificationHandler<LeagueUpdateNotification>
{
    private readonly DiscordMessageComposer _messageComposer;
    private readonly IDiscordCupMessenger _cupMessenger;

    public DiscordLeagueHandler(DiscordMessageComposer messageComposer, IDiscordCupMessenger cupMessenger)
    {
        _messageComposer = messageComposer;
        _cupMessenger = cupMessenger;
    }

    public async Task Handle(LeagueUpdateNotification notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.LeagueUpdates(notification.Updates);
        await _cupMessenger.SendMessageToCupAsync(notification.CupId, message);
    }
}