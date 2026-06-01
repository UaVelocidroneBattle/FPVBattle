using MediatR;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Leagues.Notifications;

namespace Veloci.Logic.Features.Leagues.NotificationHandlers;

public class TelegramLeagueHandler : INotificationHandler<LeagueUpdateNotification>
{
    private readonly TelegramMessageComposer _messageComposer;
    private readonly ITelegramCupMessenger _cupMessenger;

    public TelegramLeagueHandler(TelegramMessageComposer messageComposer, ITelegramCupMessenger cupMessenger)
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