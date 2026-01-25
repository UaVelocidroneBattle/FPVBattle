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
    private readonly ITelegramBotFactory _botFactory;
    private readonly ICupService _cupService;

    public TelegramMessageEventHandler(
        TelegramMessageComposer messageComposer,
        ITelegramBotFactory botFactory,
        ICupService cupService)
    {
        _messageComposer = messageComposer;
        _botFactory = botFactory;
        _cupService = cupService;
    }

    public async Task Handle(IntermediateCompetitionResult notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("No Telegram bot configured for cup {CupId}, skipping intermediate result message", cupId);
            return;
        }

        var message = _messageComposer.TempLeaderboard(notification.Leaderboard, notification.Competition.Track);
        await bot.SendMessageAsync(message);
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("No Telegram bot configured for cup {CupId}, skipping result update message", cupId);
            return;
        }

        var message = _messageComposer.TimeUpdate(notification.Deltas);
        await bot.SendMessageAsync(message);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0)
            return;

        var cupId = competition.CupId;
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("No Telegram bot configured for cup {CupId}, skipping competition stopped message", cupId);
            return;
        }

        var resultsMessage = _messageComposer.Leaderboard(competition.CompetitionResults, competition.Track.FullName);
        await bot.SendMessageAsync(resultsMessage);
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("No Telegram bot configured for cup {CupId}, skipping competition started message", cupId);
            return;
        }

        var startCompetitionMessage = _messageComposer.StartCompetition(notification.Track, notification.PilotsFlownOnTrack);
        await bot.SendMessageAsync(startCompetitionMessage);
    }

    public async Task Handle(TempSeasonResults notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.TempSeasonResults(notification.Results);
        await SendToAllCupsAsync(bot => bot.SendMessageAsync(message));
    }

    public async Task Handle(SeasonFinished notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.SeasonResults(notification.Results);
        await SendToAllCupsAsync(bot => bot.SendMessageAsync(message));

        var imageStream = new MemoryStream(notification.Image);
        await SendToAllCupsAsync(bot => bot.SendPhotoAsync(imageStream));

        var medalCountMessage = _messageComposer.MedalCount(notification.Results);
        BackgroundJob.Schedule(() => SendToAllCupsAsync(bot => bot.SendMessageAsync(medalCountMessage)), TimeSpan.FromSeconds(6));
    }

    public async Task Handle(BadTrack notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("No Telegram bot configured for cup {CupId}, skipping bad track message", cupId);
            return;
        }

        var message = _messageComposer.BadTrackRating();
        await bot.SendMessageAsync(message);
    }

    public async Task Handle(CheerUp notification, CancellationToken cancellationToken)
    {
        var cheerUpMessage = notification.Message;

        if (cheerUpMessage.FileUrl is null && cheerUpMessage.Text is not null)
        {
            await SendToAllCupsAsync(bot => bot.SendMessageAsync(cheerUpMessage.Text));
            return;
        }

        if (cheerUpMessage.FileUrl is not null)
        {
            await SendToAllCupsAsync(bot => bot.SendPhotoAsync(cheerUpMessage.FileUrl, cheerUpMessage.Text));
        }
    }

    public async Task Handle(YearResults notification, CancellationToken cancellationToken)
    {
        var messageSet = _messageComposer.YearResults(notification.Results);
        const int delaySec = 10;

        foreach (var message in messageSet)
        {
            await SendToAllCupsAsync(bot => bot.SendMessageAsync(message));
            await Task.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken);
        }
    }


    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await SendToAllCupsAsync(bot => bot.SendMessageAsync(message));
    }

    public async Task Handle(NewPilot notification, CancellationToken cancellationToken)
    {
        var cupId = notification.CupId;
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("No Telegram bot configured for cup {CupId}, skipping new pilot message", cupId);
            return;
        }

        var message = _messageComposer.NewPilot(notification.Pilot.Name);
        await bot.SendMessageAsync(message);
    }

    public async Task Handle(PilotRenamed notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.PilotRenamed(notification.OldName, notification.NewName);
        await SendToAllCupsAsync(bot => bot.SendMessageAsync(message));
    }

    public async Task Handle(EndOfSeasonStatisticsNotification notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.EndOfSeasonStatistics(notification.Statistics);
        await SendToAllCupsAsync(bot => bot.SendMessageAsync(message));
    }

    /// <summary>
    /// Sends a message to all enabled cups that have Telegram configured
    /// </summary>
    private async Task SendToAllCupsAsync(Func<ITelegramBotChannel, Task> sendAction)
    {
        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();

        foreach (var cupId in enabledCupIds)
        {
            if (_botFactory.TryGetBotForCup(cupId, out var bot))
            {
                try
                {
                    await sendAction(bot);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Failed to send message to cup {CupId}", cupId);
                }
            }
        }
    }
}
