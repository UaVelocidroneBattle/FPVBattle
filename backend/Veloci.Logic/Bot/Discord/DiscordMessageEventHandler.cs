using Hangfire;
using MediatR;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Discord;

public class DiscordMessageEventHandler :
    INotificationHandler<CompetitionStarted>,
    INotificationHandler<CurrentResultUpdated>,
    INotificationHandler<CompetitionFinished>,
    INotificationHandler<CompetitionCancelled>,
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
    INotificationHandler<TrackRestart>,
    INotificationHandler<AddedToWhitelist>,
    INotificationHandler<VoteReminder>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<DiscordMessageEventHandler>();

    private readonly DiscordMessageComposer _messageComposer;
    private readonly IDiscordBotFactory _botFactory;
    private readonly IDiscordCupMessenger _cupMessenger;
    private readonly IDiscordGeneralMessenger _generalMessenger;
    private readonly IRepository<Competition> _competitions;
    private readonly ILeaderboardCalculator _leaderboardCalculator;
    private readonly ICupService _cupService;
    private readonly DiscordChatMessages _chatMessages;

    public DiscordMessageEventHandler(
        DiscordMessageComposer messageComposer,
        IDiscordBotFactory botFactory,
        IDiscordCupMessenger cupMessenger,
        IDiscordGeneralMessenger generalMessenger,
        IRepository<Competition> competitions,
        ILeaderboardCalculator leaderboardCalculator,
        ICupService cupService,
        DiscordChatMessages chatMessages)
    {
        _messageComposer = messageComposer;
        _botFactory = botFactory;
        _cupMessenger = cupMessenger;
        _generalMessenger = generalMessenger;
        _competitions = competitions;
        _leaderboardCalculator = leaderboardCalculator;
        _cupService = cupService;
        _chatMessages = chatMessages;
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;

        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            Log.Warning("No Discord bot configured for cup {CupId}, skipping competition started message", cupId);
            return;
        }

        var track = notification.Track;
        var startMessage = _messageComposer.StartCompetition(track, notification.PilotsFlownOnTrack, notification.Competition.QuadOfTheDay?.Name);
        await bot.SendMessageAsync(startMessage);

        var leagueNames = _cupService.GetCupOptions(cupId).Leagues.GetAllLeagueNames();
        var leaderboardMessages = _messageComposer.TempLeaderboard(null, leagueNames);
        var lastKey = leaderboardMessages.Keys.Last();

        foreach (var message in leaderboardMessages)
        {
            var messageId = await bot.SendMessageAsync(message.Value);
            notification.Competition.AddOrUpdateVariable(CompetitionVariables.GetDiscordLeaderboardMessageId(message.Key), messageId.Value);

            if (message.Key == lastKey)
                notification.Competition.AddOrUpdateVariable(CompetitionVariables.DiscordTimeUpdatesMessageId, messageId.Value);
        }

        await _competitions.SaveChangesAsync(cancellationToken);
        await bot.ChangeChannelTopicAsync(notification.Track.FullName);
    }

    public async Task Handle(CurrentResultUpdated notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;

        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            Log.Warning("No Discord bot configured for cup {CupId}, skipping result update message", cupId);
            return;
        }

        var threadMessageId = notification.Competition
            .GetVariable(CompetitionVariables.DiscordTimeUpdatesMessageId)?
            .ULongValue;

        if (threadMessageId is null)
            return;

        var timeUpdateMessage = _messageComposer.TimeUpdate(notification.Deltas);
        await bot.SendMessageInThreadAsync(threadMessageId.Value, CompetitionVariables.DiscordTimeUpdatesThreadName, timeUpdateMessage);

        var leagueNames = _cupService.GetCupOptions(cupId).Leagues.GetAllLeagueNames();
        var leagueLeaderboard = _leaderboardCalculator.GetLeagueLeaderboard(notification.Competition);
        var leaderboardMessages = _messageComposer.TempLeaderboard(leagueLeaderboard, leagueNames);

        foreach (var message in leaderboardMessages)
        {
            var messageId = GetLeaderboardMessageIdForLeague(notification.Competition, message.Key);

            if (messageId is null)
                continue;

            await bot.EditMessageAsync(messageId.Value, message.Value);
        }
    }

    public async Task Handle(CompetitionFinished notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;

        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            Log.Warning("No Discord bot configured for cup {CupId}, skipping competition stopped message", cupId);
            return;
        }

        await bot.ArchiveThreadAsync(CompetitionVariables.DiscordTimeUpdatesThreadName);
        await bot.ChangeChannelTopicAsync(string.Empty);

        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0)
            return;

        var leaderboardMessages = _messageComposer.Leaderboard(notification.Leaderboard);

        foreach (var message in leaderboardMessages)
        {
            var messageId = GetLeaderboardMessageIdForLeague(competition, message.Key);

            if (messageId is null)
                continue;

            await bot.EditMessageAsync(messageId.Value, message.Value);
        }
    }

    public async Task Handle(CompetitionCancelled notification, CancellationToken cancellationToken)
    {
        var cupId = notification.Competition.CupId;

        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            Log.Warning("No Discord bot configured for cup {CupId}, skipping competition cancelled message", cupId);
            return;
        }

        await bot.ArchiveThreadAsync(CompetitionVariables.DiscordTimeUpdatesThreadName);
        await bot.ChangeChannelTopicAsync(string.Empty);
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

        await _cupMessenger.SendImageToCupAsync(notification.CupId, notification.Image, notification.ImageName);
    }

    public async Task Handle(BadTrack notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.BadTrackRating();
        await _cupMessenger.SendMessageToCupAsync(notification.Competition.CupId, message);
    }

    public async Task Handle(CheerUp notification, CancellationToken cancellationToken)
    {
        var cheerUpMessage = _chatMessages.GetRandomByTypeWithProbability(notification.Type);

        if (cheerUpMessage is null)
        {
            return;
        }

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
        await _generalMessenger.SendMessageAsync(message);
    }

    private ulong? GetLeaderboardMessageIdForLeague(Competition competition, string leagueName)
    {
        var leaderboardMessageId = competition
            .GetVariable(CompetitionVariables.GetDiscordLeaderboardMessageId(leagueName))?
            .ULongValue;

        if (leaderboardMessageId is not null)
            return leaderboardMessageId;

        Log.Error("Discord leaderboard message ID for league {League} is null for competition {CompetitionId}", leagueName, competition.Id);
        return null;
    }

    public async Task Handle(NewPilot notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.NewPilot(notification.Pilot);
        await _generalMessenger.SendMessageAsync(message);
    }

    public async Task Handle(PilotRenamed notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.PilotRenamed(notification.OldName, notification.NewName);
        await _generalMessenger.SendMessageAsync(message);
    }

    public async Task Handle(EndOfSeasonStatisticsNotification notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.EndOfSeasonStatistics(notification.Statistics);
        await _cupMessenger.SendMessageToAllCupsAsync(message);
    }

    public async Task Handle(FreezieAdded notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.FreezieAdded(notification.PilotName);
        await _generalMessenger.SendMessageAsync(message);
    }

    public async Task Handle(TrackRestart notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.RestartTrack();
        await _cupMessenger.SendMessageToCupAsync(notification.CupId, message);
    }

    public async Task Handle(AddedToWhitelist notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.AddedToWhitelist(notification.PilotName);
        await _generalMessenger.SendMessageAsync(message);
    }

    public async Task Handle(VoteReminder notification, CancellationToken cancellationToken)
    {
        var message = _chatMessages.GetRandomByType(ChatMessageType.VoteReminder);

        if (message is null)
        {
            return;
        }

        await _cupMessenger.SendMessageToCupAsync(notification.Competition.CupId, message.Text);
    }
}
