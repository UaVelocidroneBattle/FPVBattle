using FpvBattle.Application.Bot.Discord;
using FpvBattle.Application.Features.Achievements.Notifications;
using FpvBattle.Application.Features.Achievements.Services;
using MediatR;

namespace FpvBattle.Application.Features.Achievements.NotificationHandlers;

public class DiscordAchievementsHandler : INotificationHandler<GotAchievements>
{
    private readonly DiscordAchievementMessageComposer _messageComposer;
    private readonly IDiscordGeneralMessenger _generalMessenger;

    public DiscordAchievementsHandler(
        DiscordAchievementMessageComposer messageComposer,
        IDiscordGeneralMessenger generalMessenger)
    {
        _messageComposer = messageComposer;
        _generalMessenger = generalMessenger;
    }

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.AchievementList(notification.Results);
        await _generalMessenger.SendMessageAsync(message);
    }
}
