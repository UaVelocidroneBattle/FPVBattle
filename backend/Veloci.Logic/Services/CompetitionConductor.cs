using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API;
using Veloci.Logic.Bot;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Helpers;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services.Tracks;

namespace Veloci.Logic.Services;

public class CompetitionConductor
{
    private static readonly ILogger _log = Log.ForContext<CompetitionConductor>();

    private readonly Velocidrone _velocidrone;
    private readonly IRepository<Competition> _competitions;
    private readonly IRepository<Pilot> _pilots;
    private readonly TrackService _trackService;
    private readonly IMediator _mediator;
    private readonly RaceResultsConverter _resultsConverter;
    private readonly CompetitionService _competitionService;
    private readonly TelegramMessageComposer _messageComposer;
    private readonly ImageService _imageService;

    public CompetitionConductor(
        IRepository<Competition> competitions,
        RaceResultsConverter resultsConverter,
        CompetitionService competitionService,
        TelegramMessageComposer messageComposer,
        ImageService imageService,
        TrackService trackService,
        IMediator mediator,
        IRepository<Pilot> pilots,
        Velocidrone velocidrone)
    {
        _competitions = competitions;
        _resultsConverter = resultsConverter;
        _competitionService = competitionService;
        _messageComposer = messageComposer;
        _imageService = imageService;
        _trackService = trackService;
        _mediator = mediator;
        _pilots = pilots;
        _velocidrone = velocidrone;
    }

    public async Task StartNewAsync()
    {
        _log.Information("🏁 Starting a new competition");

        var activeComp = await GetActiveCompetitionAsync();

        if (activeComp is not null)
        {
            _log.Information("Found active competition {CompetitionId}, stopping poll and cancelling before starting new one", activeComp.Id);
            await StopPollAsync();
            await CancelAsync();
        }

        var track = await _trackService.GetRandomTrackAsync();
        _log.Information("🎯 Selected track {TrackName} (ID: {TrackId}) for new competition", track.Name, track.TrackId);

        var resultsDto = await _velocidrone.LeaderboardAsync(track.TrackId);
        var results = _resultsConverter.ConvertTrackTimes(resultsDto);
        _log.Debug("Retrieved {ResultCount} initial results from Velocidrone API for track {TrackId}", results.Count, track.TrackId);

        var trackResults = new TrackResults
        {
            Times = results
        };

        var competition = new Competition
        {
            TrackId = track.Id,
            State = CompetitionState.Started,
            InitialResults = trackResults,
            CurrentResults = trackResults
        };

        await _competitions.AddAsync(competition);

        var pilotsFlownOnTrack = await GetPilotsFlownOnTrackAsync(trackResults);
        _log.Information("🚀 Competition {CompetitionId} started with {PilotCount} existing pilots on track {TrackName}", competition.Id, pilotsFlownOnTrack.Count, track.Name);

        await _mediator.Publish(new CompetitionStarted(competition, track, pilotsFlownOnTrack));

        //possible needs to be moved to CompetitionStarted event handler in TelegramHandler
        await CreatePoll(track, competition);

        await _competitions.SaveChangesAsync();
        _log.Information("Competition {CompetitionId} setup completed successfully", competition.Id);
    }

    private async Task<IList<string>> GetPilotsFlownOnTrackAsync(TrackResults trackResults)
    {
        var result = new List<string>();

        foreach (var time in trackResults.Times)
        {
            var pilot = await _pilots.FindAsync(time.UserId);

            if (pilot is not null)
                result.Add(pilot.Name);
        }

        result.Sort();
        return result;
    }

    private async Task CreatePoll(Track track, Competition competition)
    {
        _log.Debug("Creating poll for track {TrackName}", track.FullName);
        var poll = _messageComposer.Poll(track.FullName);
        var pollId = await TelegramBot.SendPollAsync(poll);

        if (pollId is null)
        {
            _log.Warning("Failed to create poll for track {TrackName}", track.FullName);
            return;
        }

        _log.Information("🗳️ Created poll {PollId} for track {TrackName}", pollId.Value, track.FullName);

        var rating = competition.Track.Rating;

        if (rating is null)
        {
            rating = new TrackRating();
            competition.Track.Rating = rating;
        }

        rating.PollMessageId = pollId.Value;
    }

    public async Task StopAsync()
    {
        var competition = await GetActiveCompetitionAsync();

        if (competition is null)
            throw new Exception("There are no active competitions");

        _log.Information("Stopping competition {CompetitionId} for track {TrackName}", competition.Id, competition.Track.Name);

        competition.State = CompetitionState.Closed;
        competition.CompetitionResults = _competitionService.GetLocalLeaderboard(competition);

        _log.Information("🏁 Competition {CompetitionId} stopped with {ResultCount} final results", competition.Id, competition.CompetitionResults.Count);

        await UpdateDayStreakAsync(competition.CompetitionResults);
        await _competitions.SaveChangesAsync();

        await _competitionService.ClearTrackTimesAsync();

        await _mediator.Publish(new CompetitionStopped(competition));
        _log.Information("Competition {CompetitionId} closure process completed", competition.Id);
    }

    private async Task UpdateDayStreakAsync(List<CompetitionResults> competitionResults)
    {
        var today = DateTime.Today;
        _log.Debug("Updating day streaks for {PilotCount} pilots on {Date}", competitionResults.Count, today.ToString("yyyy-MM-dd"));

        foreach (var results in competitionResults)
        {
            var pilot = await _pilots.FindAsync(results.UserId);

            if (pilot is null)
            {
                _log.Error($"Pilot {results.UserId} not found in database, cannot update day streak");
                continue;
            }

            pilot.OnRaceFlown(today);
        }

        await _pilots.SaveChangesAsync();
        await _pilots.GetAll().ResetDayStreaksAsync(today);

        _log.Information("Updated day streaks for {PilotCount} pilots", competitionResults.Count);
    }

    private async Task CancelAsync()
    {
        var competition = await GetActiveCompetitionAsync();

        if (competition is null) throw new Exception("There are no active competitions");

        _log.Information("Cancelling a competition {competitionId}", competition.Id);

        competition.State = CompetitionState.Cancelled;
        await _competitions.SaveChangesAsync();
        await _mediator.Publish(new CompetitionCancelled(competition));
    }

    public async Task StopPollAsync()
    {
        _log.Debug("Stopping poll");

        var competition = await GetActiveCompetitionAsync();

        if (competition is null)
            throw new Exception("There are no active competitions");

        var poll = _messageComposer.Poll(competition.Track.FullName);

        if (competition.Track.Rating is null)
        {
            _log.Error("No poll to stop for competition {CompetitionId}", competition.Id);
            return;
        }

        _log.Information("Stopping poll {PollId} for track {TrackName}", competition.Track.Rating.PollMessageId, competition.Track.FullName);
        var telegramPoll = await TelegramBot.StopPollAsync(competition.Track.Rating.PollMessageId);

        if (telegramPoll is null)
        {
            _log.Error("Poll {PollId} is already stopped", competition.Track.Rating.PollMessageId);
            return;
        }

        var totalPoints = telegramPoll.Options.Sum(option =>
        {
            var points = poll.Options.FirstOrDefault(x => x.Text == option.Text).Points;
            return option.VoterCount * points;
        });

        double? rating = telegramPoll.TotalVoterCount == 0
            ? null
            : totalPoints / (double)telegramPoll.TotalVoterCount;

        _log.Information("Poll {PollId} voting completed: {VoterCount} voters, calculated rating: {Rating:F2}",
            competition.Track.Rating.PollMessageId, telegramPoll.TotalVoterCount, rating ?? 0);

        competition.Track.Rating.Value = rating;
        await _competitions.SaveChangesAsync();

        if (rating is null or >= 0)
            return;

        _log.Warning("👎 Track {TrackName} received negative rating {Rating:F2}, marking as bad track", competition.Track.FullName, rating);
        await _mediator.Publish(new BadTrack(competition, competition.Track));
    }

    public async Task SeasonResultsAsync()
    {
        var now = DateTime.Now;
        _log.Information("Processing season results for {Date} (Day {Day} of month)", now.ToString("yyyy-MM-dd"), now.Day);

        if (now.Day == 1)
        {
            _log.Information("First day of month detected, stopping the season");
            await StopSeasonAsync();
        }
        else
        {
            _log.Debug("Publishing temporary season results");
            await TempSeasonResultsAsync();
        }
    }

    public async Task VoteReminder()
    {
        _log.Debug("Publishing vote reminder");

        var competition = await GetActiveCompetitionAsync();
        var messageText = ChatMessages.GetRandomByType(ChatMessageType.VoteReminder);

        await TelegramBot.ReplyMessageAsync(messageText.Text, competition.Track.Rating.PollMessageId);
    }

    private async Task TempSeasonResultsAsync()
    {
        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var results = await _competitionService.GetSeasonResultsAsync(firstDayOfMonth, today);

        _log.Debug("Retrieved {ResultCount} results for temporary season leaderboard ({StartDate} to {EndDate})",
            results.Count, firstDayOfMonth.ToString("yyyy-MM-dd"), today.ToString("yyyy-MM-dd"));

        if (results.Count == 0)
        {
            _log.Information("No results found for current season, skipping temporary results publication");
            return;
        }

        await _mediator.Publish(new TempSeasonResults(results));
        _log.Information("Published temporary season results with {ResultCount} entries", results.Count);
    }

    private async Task StopSeasonAsync()
    {
        var today = DateTime.Now;
        var firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var seasonName = firstDayOfPreviousMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        _log.Information("Finalizing season {SeasonName} ({StartDate} to {EndDate})",
            seasonName, firstDayOfPreviousMonth.ToString("yyyy-MM-dd"), firstDayOfCurrentMonth.ToString("yyyy-MM-dd"));

        var results = await _competitionService.GetSeasonResultsAsync(firstDayOfPreviousMonth, firstDayOfCurrentMonth);

        if (results.Count == 0)
        {
            _log.Warning("No results found for season {SeasonName}, skipping season finalization", seasonName);
            return;
        }

        var winners = results
            .Take(3)
            .Select(x => x.PlayerName)
            .ToArray();

        _log.Information("Season {SeasonName} completed with {ResultCount} participants. Winners: {Winners}",
            seasonName, results.Count, string.Join(", ", winners));

        var image = await _imageService.CreateWinnerImageAsync(seasonName, winners);
        _log.Debug("Generated winner image for season {SeasonName}", seasonName);

        await _mediator.Publish(new SeasonFinished(results, seasonName, winners, image));
        _log.Information("Season {SeasonName} finalization completed", seasonName);
    }

    private async Task<Competition?> GetActiveCompetitionAsync()
    {
        return await _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .FirstOrDefaultAsync();
    }
}
