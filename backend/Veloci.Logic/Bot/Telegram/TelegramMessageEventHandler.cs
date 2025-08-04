using Hangfire;
using MediatR;
using Veloci.Logic.Features.Patreon.Notifications;
using Veloci.Logic.Helpers;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram;

public class TelegramMessageEventHandler :
    INotificationHandler<IntermediateCompetitionResult>,
    INotificationHandler<CurrentResultUpdateMessage>,
    INotificationHandler<CompetitionStopped>,
    INotificationHandler<CompetitionStarted>,
    INotificationHandler<TempSeasonResults>,
    INotificationHandler<SeasonFinished>,
    INotificationHandler<BadTrack>,
    INotificationHandler<CheerUp>,
    INotificationHandler<YearResults>,
    INotificationHandler<DayStreakAchievements>,
    INotificationHandler<DayStreakPotentialLose>,
    INotificationHandler<GotAchievements>,
    INotificationHandler<NewPatreonSupporterNotification>,
    INotificationHandler<MonthlyPatreonSupportersNotification>
{
    private readonly TelegramMessageComposer _messageComposer;

    public TelegramMessageEventHandler(TelegramMessageComposer messageComposer)
    {
        _messageComposer = messageComposer;
    }

    public async Task Handle(IntermediateCompetitionResult notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempLeaderboard(notification.Leaderboard, notification.Competition.Track);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TimeUpdate(notification.Deltas);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0)
            return;

        var resultsMessage = _messageComposer.Leaderboard(competition.CompetitionResults, competition.Track.FullName);
        await TelegramBot.SendMessageAsync(resultsMessage);
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var startCompetitionMessage = _messageComposer.StartCompetition(notification.Track, notification.PilotsFlownOnTrack);
        await TelegramBot.SendMessageAsync(startCompetitionMessage);
    }

    public async Task Handle(TempSeasonResults notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempSeasonResults(notification.Results);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.SeasonResults(notification.Results);
        await TelegramBot.SendMessageAsync(message);

        await TelegramBot.SendPhotoAsync(new MemoryStream(notification.Image));

        var medalCountMessage = _messageComposer.MedalCount(notification.Results);
        BackgroundJob.Schedule(() => TelegramBot.SendMessageAsync(medalCountMessage), TimeSpan.FromSeconds(6));
    }

    public async Task Handle(BadTrack notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.BadTrackRating();
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(CheerUp notification, CancellationToken cancellationToken)
    {
        var cheerUpMessage = notification.Message;

        if (cheerUpMessage.FileUrl is null && cheerUpMessage.Text is not null)
        {
            await TelegramBot.SendMessageAsync(cheerUpMessage.Text);
            return;
        }

        if (cheerUpMessage.FileUrl is not null)
        {
            await TelegramBot.SendPhotoAsync(cheerUpMessage.FileUrl, cheerUpMessage.Text);
        }
    }

    public async Task Handle(YearResults notification, CancellationToken cancellationToken)
    {
        var messageSet = _messageComposer.YearResults(notification.Results);
        const int delaySec = 10;

        foreach (var message in messageSet)
        {
            await TelegramBot.SendMessageAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(DayStreakAchievements notification, CancellationToken cancellationToken)
    {
        const int delaySec = 3;

        foreach (var pilot in notification.Pilots)
        {
            var message = _messageComposer.DayStreakAchievement(pilot);
            await TelegramBot.SendMessageAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.AchievementList(notification.Results);
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

    public async Task Handle(MonthlyPatreonSupportersNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Supporters.Any())
            return;

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
}
