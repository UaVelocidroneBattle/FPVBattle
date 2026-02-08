using Hangfire;
using MediatR;
using Veloci.Logic.Helpers;
using Veloci.Logic.Notifications;

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
    private readonly ITelegramCupMessenger _cupMessenger;

    public TelegramMessageEventHandler(
        TelegramMessageComposer messageComposer,
        ITelegramCupMessenger cupMessenger)
    {
        _messageComposer = messageComposer;
        _cupMessenger = cupMessenger;
    }

    public async Task Handle(IntermediateCompetitionResult notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempLeaderboard(notification.Leaderboard, notification.Competition.Track);
        await _cupMessenger.SendMessageToCupAsync(notification.Competition.CupId, message);
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TimeUpdate(notification.Deltas);
        await _cupMessenger.SendMessageToCupAsync(notification.Competition.CupId, message);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0)
            return;

        var resultsMessage = _messageComposer.Leaderboard(competition.CompetitionResults, competition.Track.FullName);
        await _cupMessenger.SendMessageToCupAsync(competition.CupId, resultsMessage);
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var startCompetitionMessage = _messageComposer.StartCompetition(notification.Track, notification.PilotsFlownOnTrack);
        await _cupMessenger.SendMessageToCupAsync(notification.Competition.CupId, startCompetitionMessage);
    }

    public async Task Handle(TempSeasonResults notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempSeasonResults(notification.Results);
        await _cupMessenger.SendMessageToCupAsync(notification.CupId, message);
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.SeasonResults(notification.Results);
        await _cupMessenger.SendMessageToCupAsync(notification.CupId, message);

        var imageStream = new MemoryStream(notification.Image);
        await _cupMessenger.SendPhotoToCupAsync(notification.CupId, imageStream);

        var medalCountMessage = _messageComposer.MedalCount(notification.Results);
        BackgroundJob.Schedule(() => SendMedalCountToCupAsync(notification.CupId, medalCountMessage), TimeSpan.FromSeconds(6));
    }

    public async Task Handle(BadTrack notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.BadTrackRating();
        await _cupMessenger.SendMessageToCupAsync(notification.Competition.CupId, message);
    }

    public async Task Handle(CheerUp notification, CancellationToken cancellationToken)
    {
        var cheerUpMessage = notification.Message;
        await _cupMessenger.SendMessageToCupAsync(notification.CupId, cheerUpMessage.Text);
    }

    public async Task Handle(YearResults notification, CancellationToken cancellationToken)
    {
        var messageSet = _messageComposer.YearResults(notification.Results);
        const int delaySec = 10;

        foreach (var message in messageSet)
        {
            await _cupMessenger.SendMessageToAllCupsAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }


    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(NewPilot notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.NewPilot(notification.Pilot.Name);
        await _cupMessenger.SendMessageToCupAsync(notification.CupId, message);
    }

    public async Task Handle(PilotRenamed notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.PilotRenamed(notification.OldName, notification.NewName);
        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(EndOfSeasonStatisticsNotification notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.EndOfSeasonStatistics(notification.Statistics);
        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }

    /// <summary>
    /// Public method for Hangfire to call - sends medal count message to a specific cup
    /// </summary>
    public async Task SendMedalCountToCupAsync(string cupId, string message)
    {
        await _cupMessenger.SendMessageToCupAsync(cupId, message);
    }

    public async Task Handle(FreezieAdded notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.FreezieAdded(notification.PilotName);
        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(TrackRestart notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.RestartTrack();
        await _cupMessenger.SendMessageToCupAsync(notification.CupId, message);
    }
}
