using Hangfire;
using MediatR;
using Serilog;
using Veloci.Logic.Features.Cups;
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
    INotificationHandler<EndOfSeasonStatisticsNotification>
{
    private static readonly ILogger _log = Log.ForContext<TelegramMessageEventHandler>();

    private readonly TelegramMessageComposer _messageComposer;
    private readonly ITelegramMessenger _messenger;
    private readonly ICupService _cupService;

    public TelegramMessageEventHandler(
        TelegramMessageComposer messageComposer,
        ITelegramMessenger messenger,
        ICupService cupService)
    {
        _messageComposer = messageComposer;
        _messenger = messenger;
        _cupService = cupService;
    }

    public async Task Handle(IntermediateCompetitionResult notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        var channelId = _cupService.GetTelegramChannelId(cupId);
        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("No Telegram channel configured for cup {CupId}, skipping intermediate result message", cupId);
            return;
        }

        var message = _messageComposer.TempLeaderboard(notification.Leaderboard, notification.Competition.Track);
        await _messenger.SendMessageAsync(channelId, message);
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        var channelId = _cupService.GetTelegramChannelId(cupId);
        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("No Telegram channel configured for cup {CupId}, skipping result update message", cupId);
            return;
        }

        var message = _messageComposer.TimeUpdate(notification.Deltas);
        await _messenger.SendMessageAsync(channelId, message);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0)
            return;

        var cupId = competition.CupId;
        var channelId = _cupService.GetTelegramChannelId(cupId);
        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("No Telegram channel configured for cup {CupId}, skipping competition stopped message", cupId);
            return;
        }

        var resultsMessage = _messageComposer.Leaderboard(competition.CompetitionResults, competition.Track.FullName);
        await _messenger.SendMessageAsync(channelId, resultsMessage);
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        var channelId = _cupService.GetTelegramChannelId(cupId);
        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("No Telegram channel configured for cup {CupId}, skipping competition started message", cupId);
            return;
        }

        var startCompetitionMessage = _messageComposer.StartCompetition(notification.Track, notification.PilotsFlownOnTrack);
        await _messenger.SendMessageAsync(channelId, startCompetitionMessage);
    }

    public async Task Handle(TempSeasonResults notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempSeasonResults(notification.Results);
        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.SeasonResults(notification.Results);
        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));

        var imageStream = new MemoryStream(notification.Image);
        await SendToAllCupsAsync(channelId => _messenger.SendPhotoAsync(channelId, imageStream));

        var medalCountMessage = _messageComposer.MedalCount(notification.Results);
        BackgroundJob.Schedule(() => SendMedalCountToAllCupsAsync(medalCountMessage), TimeSpan.FromSeconds(6));
    }

    public async Task Handle(BadTrack notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        var channelId = _cupService.GetTelegramChannelId(cupId);
        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("No Telegram channel configured for cup {CupId}, skipping bad track message", cupId);
            return;
        }

        var message = _messageComposer.BadTrackRating();
        await _messenger.SendMessageAsync(channelId, message);
    }

    public async Task Handle(CheerUp notification, CancellationToken cancellationToken)
    {
        var cheerUpMessage = notification.Message;

        if (cheerUpMessage.FileUrl is null && cheerUpMessage.Text is not null)
        {
            await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, cheerUpMessage.Text));
            return;
        }

        if (cheerUpMessage.FileUrl is not null)
        {
            await SendToAllCupsAsync(channelId => _messenger.SendPhotoAsync(channelId, cheerUpMessage.FileUrl, cheerUpMessage.Text));
        }
    }

    public async Task Handle(YearResults notification, CancellationToken cancellationToken)
    {
        var messageSet = _messageComposer.YearResults(notification.Results);
        const int delaySec = 10;

        foreach (var message in messageSet)
        {
            await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }


    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public async Task Handle(NewPilot notification, CancellationToken cancellationToken)
    {
        var cupId = notification.CupId;
        var channelId = _cupService.GetTelegramChannelId(cupId);
        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("No Telegram channel configured for cup {CupId}, skipping new pilot message", cupId);
            return;
        }

        var message = _messageComposer.NewPilot(notification.Pilot.Name);
        await _messenger.SendMessageAsync(channelId, message);
    }

    public async Task Handle(PilotRenamed notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.PilotRenamed(notification.OldName, notification.NewName);
        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public async Task Handle(EndOfSeasonStatisticsNotification notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.EndOfSeasonStatistics(notification.Statistics);
        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    /// <summary>
    /// Public method for Hangfire to call - sends medal count message to all cups
    /// </summary>
    public async Task SendMedalCountToAllCupsAsync(string message)
    {
        await SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    /// <summary>
    /// Sends a message to all enabled cups that have Telegram configured
    /// </summary>
    private async Task SendToAllCupsAsync(Func<string, Task> sendAction)
    {
        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();

        foreach (var cupId in enabledCupIds)
        {
            var channelId = _cupService.GetTelegramChannelId(cupId);
            if (!string.IsNullOrEmpty(channelId))
            {
                try
                {
                    await sendAction(channelId);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Failed to send message to cup {CupId}", cupId);
                }
            }
        }
    }
}
