using Hangfire;
using MediatR;
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
    INotificationHandler<DayStreakPotentialLose>,
    INotificationHandler<NewPilot>,
    INotificationHandler<PilotRenamed>,
    INotificationHandler<EndOfSeasonStatisticsNotification>,
    INotificationHandler<FreezieAdded>,
    INotificationHandler<TrackRestart>
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


    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(NewPilot notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.NewPilot(notification.Pilot.Name);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(PilotRenamed notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.PilotRenamed(notification.OldName, notification.NewName);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(EndOfSeasonStatisticsNotification notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.EndOfSeasonStatistics(notification.Statistics);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(FreezieAdded notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.FreezieAdded(notification.PilotName);
        await TelegramBot.SendMessageAsync(message);
    }

    public async Task Handle(TrackRestart notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.RestartTrack();
        await TelegramBot.SendMessageAsync(message);
    }
}
