using Hangfire;
using MediatR;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Helpers;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Discord;

public class DiscordMessageEventHandler :
    INotificationHandler<CompetitionStarted>,
    INotificationHandler<CurrentResultUpdateMessage>,
    INotificationHandler<CompetitionStopped>,
    INotificationHandler<CompetitionCancelled>,
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
    private static readonly ILogger _log = Log.ForContext<DiscordMessageEventHandler>();

    private readonly DiscordMessageComposer _messageComposer;
    private readonly IDiscordBot _discordBot;
    private readonly IRepository<Competition> _competitions;
    private readonly CompetitionService _competitionService;

    public DiscordMessageEventHandler(
        DiscordMessageComposer messageComposer,
        IDiscordBot discordBot,
        IRepository<Competition> competitions,
        CompetitionService competitionService)
    {
        _messageComposer = messageComposer;
        _discordBot = discordBot;
        _competitions = competitions;
        _competitionService = competitionService;
    }

    public async Task Handle(CompetitionStarted notification, CancellationToken cancellationToken)
    {
        var track = notification.Track;
        var startMessage = _messageComposer.StartCompetition(track, notification.PilotsFlownOnTrack);
        await _discordBot.SendMessageAsync(startMessage);

        var leaderboardMessage = _messageComposer.TempLeaderboard(null);
        var messageId = await _discordBot.SendMessageAsync(leaderboardMessage);
        notification.Competition.AddOrUpdateVariable(CompetitionVariables.DiscordLeaderboardMessageId, messageId.Value);
        await _competitions.SaveChangesAsync(cancellationToken);

        await _discordBot.ChangeChannelTopicAsync(notification.Track.FullName);
    }

    public async Task Handle(CurrentResultUpdateMessage notification, CancellationToken cancellationToken)
    {
        var leaderboardMessageId = GetLeaderboardMessageId(notification.Competition);

        if (leaderboardMessageId is null)
            return;

        var message = _messageComposer.TimeUpdate(notification.Deltas);
        await _discordBot.SendMessageInThreadAsync(leaderboardMessageId.Value, CompetitionVariables.DiscordTimeUpdatesThreadName, message);

        var leaderboard = _competitionService.GetLocalLeaderboard(notification.Competition);
        var leaderboardMessage = _messageComposer.TempLeaderboard(leaderboard);
        await _discordBot.EditMessageAsync(leaderboardMessageId.Value, leaderboardMessage);
    }

    public async Task Handle(CompetitionStopped notification, CancellationToken cancellationToken)
    {
        await _discordBot.ArchiveThreadAsync(CompetitionVariables.DiscordTimeUpdatesThreadName);
        await _discordBot.ChangeChannelTopicAsync(string.Empty);

        var competition = notification.Competition;

        if (competition.CompetitionResults.Count == 0)
            return;

        var leaderboardMessageId = GetLeaderboardMessageId(competition);

        if (leaderboardMessageId is null)
            return;

        var resultsMessage = _messageComposer.Leaderboard(competition.CompetitionResults);
        await _discordBot.EditMessageAsync(leaderboardMessageId.Value, resultsMessage);
    }

    public async Task Handle(CompetitionCancelled notification, CancellationToken cancellationToken)
    {
        await _discordBot.ArchiveThreadAsync(CompetitionVariables.DiscordTimeUpdatesThreadName);
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

        await _discordBot.SendImageAsync(notification.Image);

        var medalCountMessage = _messageComposer.MedalCount(notification.Results);
        BackgroundJob.Schedule(() => _discordBot.SendMessageAsync(medalCountMessage), TimeSpan.FromSeconds(6));
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


    public async Task Handle(DayStreakPotentialLose notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.DayStreakPotentialLose(notification.Pilots);
        await _discordBot.SendMessageAsync(message);
    }

    private ulong? GetLeaderboardMessageId(Competition competition)
    {
        var leaderboardMessageId = competition
            .GetVariable(CompetitionVariables.DiscordLeaderboardMessageId)?
            .ULongValue;

        if (leaderboardMessageId is not null)
            return leaderboardMessageId;

        _log.Error("Discord leaderboard message ID is null for competition {CompetitionId}", competition.Id);
        return null;
    }

    public async Task Handle(NewPilot notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.NewPilot(notification.Pilot.Name);
        await _discordBot.SendMessageAsync(message);
    }

    public async Task Handle(PilotRenamed notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.PilotRenamed(notification.OldName, notification.NewName);
        await _discordBot.SendMessageAsync(message);
    }

    public async Task Handle(EndOfSeasonStatisticsNotification notification, CancellationToken cancellationToken)
    {
        var message = _messageComposer.EndOfSeasonStatistics(notification.Statistics);
        await _discordBot.SendMessageAsync(message);
    }
}
