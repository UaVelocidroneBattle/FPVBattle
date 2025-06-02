using MediatR;
using Veloci.Logic.Helpers;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot;

public class DiscordCompetitionStartedHandler : INotificationHandler<CompetitionStarted>
{
    private readonly IDiscordBot _discordBot;

    public DiscordCompetitionStartedHandler(IDiscordBot discordBot)
    {
        _discordBot = discordBot;
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var track = notification.Track;

        var message = Environment.NewLine +
            $"📅 Вітаємо на щоденному *UA Velocidrone Battle*!{Environment.NewLine}{Environment.NewLine}" +
            $"Трек дня:{Environment.NewLine}" +
            $"*{track.Map.Name}* - `{track.Name}`{Environment.NewLine}{Environment.NewLine}" +
            $"Leaderboard:{Environment.NewLine}" +
            $"<https://www.velocidrone.com/leaderboard/{track.Map.MapId}/{track.TrackId}/All>{Environment.NewLine}{Environment.NewLine}" +
            $"Тицяй, щоб скопіювати ⬇️{Environment.NewLine}" +
            $"```{Environment.NewLine}{track.Name.Trim()}{Environment.NewLine}```{Environment.NewLine}";

        await _discordBot.SendMessage(message);
    }
}

public class DiscordMessageEventHandler :
    INotificationHandler<IntermediateCompetitionResult>,
    INotificationHandler<CurrentResultUpdateMessage>,
    INotificationHandler<CompetitionStopped>,
    INotificationHandler<TempSeasonResults>,
    INotificationHandler<SeasonFinished>,
    INotificationHandler<BadTrack>,
    INotificationHandler<CheerUp>,
    INotificationHandler<YearResults>,
    INotificationHandler<DayStreakAchievements>,
    INotificationHandler<DayStreakPotentialLose>
{
    private readonly MessageComposer _messageComposer;
    private readonly IDiscordBot _discordBot;

    public DiscordMessageEventHandler(MessageComposer messageComposer, IDiscordBot discordBot)
    {
        _messageComposer = messageComposer;
        _discordBot = discordBot;
    }

    public async Task Handle(IntermediateCompetitionResult notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempLeaderboard(notification.Leaderboard, notification.Competition.Track);
        await _discordBot.SendMessage(message);
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TimeUpdate(notification.Deltas);
        await _discordBot.SendMessage(message);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0) return;

        var resultsMessage = _messageComposer.Leaderboard(competition.CompetitionResults, competition.Track.FullName, false);
        await _discordBot.SendMessage(resultsMessage);
    }

    public async Task Handle(TempSeasonResults notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempSeasonResults(notification.Results, false);
        await _discordBot.SendMessage(message);
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.SeasonResults(notification.Results);
        await _discordBot.SendMessage(message);

        var medalCountMessage = _messageComposer.MedalCount(notification.Results);
        await _discordBot.SendMessage(medalCountMessage);
    }

    public async Task Handle(BadTrack notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.BadTrackRating();
        await _discordBot.SendMessage(message);
    }

    public async Task Handle(CheerUp notification, CancellationToken cancellationToken)
    {
        var cheerUpMessage = notification.Message;

        if (cheerUpMessage.FileUrl is null && cheerUpMessage.Text is not null)
        {
            await _discordBot.SendMessage(cheerUpMessage.Text);
            return;
        }
        // if (cheerUpMessage.FileUrl is not null)
        // {
        //     await TelegramBot.SendPhotoAsync(cheerUpMessage.FileUrl, cheerUpMessage.Text);
        // }
    }

    public async Task Handle(YearResults notification, CancellationToken cancellationToken)
    {
        var messageSet = _messageComposer.YearResults(notification.Results);
        const int delaySec = 10;

        foreach (var message in messageSet)
        {
            await _discordBot.SendMessage(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(DayStreakAchievements notification, CancellationToken cancellationToken)
    {
        const int delaySec = 3;

        foreach (var pilot in notification.Pilots)
        {
            var message = _messageComposer.DayStreakAchievement(pilot);
            await _discordBot.SendMessage(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await _discordBot.SendMessage(message);
    }
}
