using MediatR;
using Veloci.Logic.Helpers;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Bot.Discord;

public class DiscordMessageEventHandler :
    INotificationHandler<CompetitionStarted>,
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
    private readonly DiscordMessageComposer _messageComposer;
    private readonly IDiscordBot _discordBot;

    public DiscordMessageEventHandler(DiscordMessageComposer messageComposer, IDiscordBot discordBot)
    {
        _messageComposer = messageComposer;
        _discordBot = discordBot;
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var track = notification.Track;
        var message = _messageComposer.StartCompetition(track);
        await _discordBot.SendMessageAsync(message);
        await _discordBot.ChangeChannelTopicAsync(notification.Track.FullName);
    }

    public async Task Handle(IntermediateCompetitionResult notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempLeaderboard(notification.Leaderboard);
        await _discordBot.SendMessageAsync(message);
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TimeUpdate(notification.Deltas);
        await _discordBot.SendMessageAsync(message);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0) return;

        var resultsMessage = _messageComposer.Leaderboard(competition.CompetitionResults, competition.Track.FullName, false);
        await _discordBot.SendMessageAsync(resultsMessage);
        await _discordBot.ChangeChannelTopicAsync(string.Empty);
    }

    public async Task Handle(TempSeasonResults notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempSeasonResults(notification.Results, false);
        await _discordBot.SendMessageAsync(message);
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.SeasonResults(notification.Results);
        await _discordBot.SendMessageAsync(message);

        var medalCountMessage = _messageComposer.MedalCount(notification.Results);
        await _discordBot.SendMessageAsync(medalCountMessage);
    }

    public async Task Handle(BadTrack notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.BadTrackRating();
        await _discordBot.SendMessageAsync(message);
    }

    public async Task Handle(CheerUp notification, CancellationToken cancellationToken)
    {
        var cheerUpMessage = notification.Message;

        if (cheerUpMessage.FileUrl is null && cheerUpMessage.Text is not null)
        {
            await _discordBot.SendMessageAsync(cheerUpMessage.Text);
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
            await _discordBot.SendMessageAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(DayStreakAchievements notification, CancellationToken cancellationToken)
    {
        const int delaySec = 3;

        foreach (var pilot in notification.Pilots)
        {
            var message = _messageComposer.DayStreakAchievement(pilot);
            await _discordBot.SendMessageAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }

    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await _discordBot.SendMessageAsync(message);
    }
}
