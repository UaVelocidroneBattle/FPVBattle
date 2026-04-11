using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API;
using Veloci.Logic.API.Dto;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.QuadOfTheDay;
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
    private readonly ICupService _cupService;
    private readonly ITelegramCupMessenger _telegramCupMessenger;
    private readonly TrackQueueService _trackQueueService;
    private readonly QuadOfTheDayService _quadOfTheDayService;

    public CompetitionConductor(
        IRepository<Competition> competitions,
        RaceResultsConverter resultsConverter,
        CompetitionService competitionService,
        TelegramMessageComposer messageComposer,
        ImageService imageService,
        TrackService trackService,
        IMediator mediator,
        IRepository<Pilot> pilots,
        Velocidrone velocidrone,
        ICupService cupService,
        ITelegramCupMessenger telegramCupMessenger,
        TrackQueueService trackQueueService,
        QuadOfTheDayService quadOfTheDayService)
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
        _cupService = cupService;
        _telegramCupMessenger = telegramCupMessenger;
        _trackQueueService = trackQueueService;
        _quadOfTheDayService = quadOfTheDayService;
    }

    public async Task StartNewAsync(string cupId)
    {
        _log.Information("🏁 Starting a new competition for cup {CupId}", cupId);

        var cupOptions = _cupService.GetCupOptions(cupId);

        if (!cupOptions.IsEnabled)
        {
            _log.Warning("Cup {CupId} is disabled, skipping competition start", cupId);
            return;
        }

        var activeComp = await GetActiveCompetitionAsync(cupId);

        if (activeComp is not null)
        {
            _log.Information("Found active competition {CompetitionId} in cup {CupId}, stopping poll and cancelling before starting new one", activeComp.Id, cupId);
            await StopPollAsync(cupId);
            await CancelAsync(cupId);
        }

        ICollection<TrackTimeDto> resultsDto;

        Track track;
        QuadModel? quad = null;

        var queuedTrack = await _trackQueueService.TryDequeueNextTrackAsync(cupId);

        if (queuedTrack is not null)
        {
            track = queuedTrack.Track;
            quad = queuedTrack.Quad;
            resultsDto = await _velocidrone.LeaderboardAsync(track.TrackId);
        }
        else
        {
            const int maxAttempts = 10;
            var trackFilter = new TrackFilter(cupOptions.TrackFilter);
            var attempts = 0;

            do
            {
                track = await _trackService.GetRandomTrackAsync(trackFilter);
                attempts++;
                _log.Information("🎯 Selected track {TrackName} (ID: {TrackId}) for cup {CupId} (attempt {Attempt}/{MaxAttempts})", track.Name, track.TrackId, cupId, attempts, maxAttempts);
                resultsDto = await _velocidrone.LeaderboardAsync(track.TrackId);

                if (resultsDto.Count == 0)
                    _log.Warning("Track {TrackName} has no results, selecting another track", track.Name);

            } while (resultsDto.Count == 0 && attempts < maxAttempts);

            if (resultsDto.Count == 0)
                throw new InvalidOperationException($"No track with results found for cup {cupId} after {maxAttempts} attempts");
        }

        quad ??= await _quadOfTheDayService.GetQuadOfTheDayAsync(cupOptions);

        var results = await _resultsConverter.ConvertTrackTimesAsync(resultsDto, cupOptions.QuadClasses, quad);
        _log.Debug("Retrieved {ResultCount} initial results from Velocidrone API for track {TrackId}", results.Count, track.TrackId);

        var trackResults = new TrackResults
        {
            Times = results
        };

        var competition = new Competition
        {
            CupId = cupId,
            TrackId = track.Id,
            State = CompetitionState.Started,
            InitialResults = trackResults,
            CurrentResults = trackResults,
        };

        competition.QuadOfTheDay = quad;

        await _competitions.AddAsync(competition);

        var pilotsFlownOnTrack = await GetPilotsFlownOnTrackAsync(trackResults);
        _log.Information("🚀 Competition {CompetitionId} started for cup {CupId} with {PilotCount} existing pilots on track {TrackName}", competition.Id, cupId, pilotsFlownOnTrack.Count, track.Name);

        await _mediator.Publish(new CompetitionStarted(competition, track, pilotsFlownOnTrack, cupOptions));

        //TODO: possible needs to be moved to CompetitionStarted event handler in TelegramHandler
        await CreatePoll(track, competition);

        await _competitions.SaveChangesAsync();
        _log.Information("Competition {CompetitionId} setup completed successfully for cup {CupId}", competition.Id, cupId);
    }

    private async Task<IList<string>> GetPilotsFlownOnTrackAsync(TrackResults trackResults)
    {
        var userIds = trackResults.Times
            .Where(t => t.UserId.HasValue)
            .Select(t => t.UserId!.Value)
            .ToList();

        var names = await _pilots
            .GetAll(p => userIds.Contains(p.Id))
            .Select(p => p.Name)
            .ToListAsync();

        names.Sort();
        return names;
    }

    private async Task CreatePoll(Track track, Competition competition)
    {
        _log.Debug("Creating poll for track {TrackName} in cup {CupId}", track.FullName, competition.CupId);

        var poll = _messageComposer.Poll(track.FullName);
        var pollId = await _telegramCupMessenger.SendPollToCupAsync(competition.CupId, poll);

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

    public async Task StopAsync(string cupId)
    {
        var competition = await GetActiveCompetitionAsync(cupId);

        if (competition is null)
        {
            _log.Warning("No active competition found in cup '{CupId}', skipping stop", cupId);
            return;
        }

        _log.Information("Stopping competition {CompetitionId} for track {TrackName} in cup {CupId}", competition.Id, competition.Track.Name, cupId);

        var cupOptions = _cupService.GetCupOptions(cupId);

        competition.State = CompetitionState.Closed;
        competition.CompetitionResults = _competitionService.GetLocalLeaderboard(competition);
        _quadOfTheDayService.PunishNonQuadOfTheDayPilots(competition);

        _log.Information("🏁 Competition {CompetitionId} stopped with {ResultCount} final results in cup {CupId}", competition.Id, competition.CompetitionResults.Count, cupId);

        await _competitions.SaveChangesAsync();

        await _mediator.Publish(new CompetitionFinished(competition, cupOptions));
        _log.Information("Competition {CompetitionId} closure process completed for cup {CupId}", competition.Id, cupId);
    }

    private async Task CancelAsync(string cupId)
    {
        var competition = await GetActiveCompetitionAsync(cupId);

        if (competition is null) throw new Exception($"There are no active competitions in cup {cupId}");

        _log.Information("Cancelling a competition {competitionId} in cup {CupId}", competition.Id, cupId);

        competition.State = CompetitionState.Cancelled;
        await _competitions.SaveChangesAsync();
        await _mediator.Publish(new CompetitionCancelled(competition));
    }

    public async Task StopPollAsync(string cupId)
    {
        _log.Debug("Stopping poll for cup {CupId}", cupId);

        var competition = await GetActiveCompetitionAsync(cupId);

        if (competition is null)
        {
            _log.Warning("No active competition found in cup '{CupId}', skipping poll stop", cupId);
            return;
        }

        var poll = _messageComposer.Poll(competition.Track.FullName);

        if (competition.Track.Rating is null)
        {
            _log.Error("No poll to stop for competition {CompetitionId}", competition.Id);
            return;
        }

        _log.Information("Stopping poll {PollId} for track {TrackName}", competition.Track.Rating.PollMessageId, competition.Track.FullName);
        var telegramPoll = await _telegramCupMessenger.StopPollInCupAsync(cupId, competition.Track.Rating.PollMessageId);

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

    public async Task SeasonResultsAsync(string cupId)
    {
        var now = DateTime.Now;
        _log.Information("Processing season results for {Date} (Day {Day} of month)", now.ToString("yyyy-MM-dd"), now.Day);

        if (now.Day == 1)
        {
            _log.Information("First day of month detected, stopping the season for cup {CupId}", cupId);
            await StopSeasonAsync(cupId);
        }
        else
        {
            _log.Debug("Publishing temporary season results for cup {CupId}", cupId);
            await TempSeasonResultsAsync(cupId);
        }
    }

    public async Task VoteReminder(string cupId)
    {
        _log.Debug("Publishing vote reminder for cup {CupId}", cupId);

        var competition = await GetActiveCompetitionAsync(cupId);

        if (competition is null)
        {
            _log.Warning("No active competition found for vote reminder in cup {CupId}", cupId);
            return;
        }

        await _mediator.Publish(new VoteReminder(competition));
    }

    private async Task TempSeasonResultsAsync(string cupId)
    {
        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var results = await _competitionService.GetSeasonResultsAsync(cupId, firstDayOfMonth, today);

        _log.Debug("Retrieved {ResultCount} results for temporary season leaderboard for cup {CupId} ({StartDate} to {EndDate})",
            results.Count, cupId, firstDayOfMonth.ToString("yyyy-MM-dd"), today.ToString("yyyy-MM-dd"));

        if (results.Count == 0)
        {
            _log.Information("No results found for current season in cup {CupId}, skipping temporary results publication", cupId);
            return;
        }

        await _mediator.Publish(new TempSeasonResults(cupId, results));
        _log.Information("Published temporary season results for cup {CupId} with {ResultCount} entries", cupId, results.Count);
    }

    private async Task StopSeasonAsync(string cupId)
    {
        var today = DateTime.Now;
        var firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var seasonName = firstDayOfPreviousMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        _log.Information("Finalizing season {SeasonName} for cup {CupId} ({StartDate} to {EndDate})",
            seasonName, cupId, firstDayOfPreviousMonth.ToString("yyyy-MM-dd"), firstDayOfCurrentMonth.ToString("yyyy-MM-dd"));

        var results = await _competitionService.GetSeasonResultsAsync(cupId, firstDayOfPreviousMonth, firstDayOfCurrentMonth);

        if (results.Count == 0)
        {
            _log.Warning("No results found for season {SeasonName} in cup {CupId}, skipping season finalization", seasonName, cupId);
            return;
        }

        var winners = results
            .Take(3)
            .Select(x => x.PlayerName)
            .ToArray();

        _log.Information("Season {SeasonName} for cup {CupId} completed with {ResultCount} participants. Winners: {Winners}",
            seasonName, cupId, results.Count, string.Join(", ", winners));

        var image = await _imageService.CreateWinnerImageAsync(seasonName, winners);
        _log.Debug("Generated winner image for season {SeasonName} cup {CupId}", seasonName, cupId);

        await _mediator.Publish(new SeasonFinished(cupId, results, seasonName, winners, image, "winners.png"));
        _log.Information("Season {SeasonName} finalization completed for cup {CupId}", seasonName, cupId);
    }

    private async Task<Competition?> GetActiveCompetitionAsync(string cupId)
    {
        return await _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .ForCup(cupId)
            .FirstOrDefaultAsync();
    }
}
