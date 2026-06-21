using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API;
using Veloci.Logic.API.Dto;
using Veloci.Logic.Bot.Discord;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.QuadOfTheDay;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services.Tracks;

namespace Veloci.Logic.Services;

public class CompetitionConductor
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CompetitionConductor>();

    private readonly Velocidrone _velocidrone;
    private readonly IRepository<Competition> _competitions;
    private readonly IRepository<Pilot> _pilots;
    private readonly TrackService _trackService;
    private readonly IMediator _mediator;
    private readonly RaceResultsConverter _resultsConverter;
    private readonly DiscordMessageComposer _discordMessageComposer;
    private readonly ImageService _imageService;
    private readonly ICupService _cupService;
    private readonly IDiscordCupMessenger _discordCupMessenger;
    private readonly TrackQueueService _trackQueueService;
    private readonly QuadOfTheDayService _quadOfTheDayService;
    private readonly ILeaderboardCalculator _leaderboardCalculator;

    public CompetitionConductor(
        IRepository<Competition> competitions,
        RaceResultsConverter resultsConverter,
        DiscordMessageComposer discordMessageComposer,
        ImageService imageService,
        TrackService trackService,
        IMediator mediator,
        IRepository<Pilot> pilots,
        Velocidrone velocidrone,
        ICupService cupService,
        IDiscordCupMessenger discordCupMessenger,
        TrackQueueService trackQueueService,
        QuadOfTheDayService quadOfTheDayService,
        ILeaderboardCalculator leaderboardCalculator)
    {
        _competitions = competitions;
        _resultsConverter = resultsConverter;
        _discordMessageComposer = discordMessageComposer;
        _imageService = imageService;
        _trackService = trackService;
        _mediator = mediator;
        _pilots = pilots;
        _velocidrone = velocidrone;
        _cupService = cupService;
        _discordCupMessenger = discordCupMessenger;
        _trackQueueService = trackQueueService;
        _quadOfTheDayService = quadOfTheDayService;
        _leaderboardCalculator = leaderboardCalculator;
    }

    public async Task StartNewAsync(string cupId)
    {
        Log.Information("🏁 Starting a new competition for cup {CupId}", cupId);

        var cupOptions = _cupService.GetCupOptions(cupId);

        if (!cupOptions.IsEnabled)
        {
            Log.Warning("Cup {CupId} is disabled, skipping competition start", cupId);
            return;
        }

        var activeComp = await GetActiveCompetitionAsync(cupId);

        if (activeComp is not null)
        {
            Log.Information("Found active competition {CompetitionId} in cup {CupId}, stopping poll and cancelling before starting new one", activeComp.Id, cupId);
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
                Log.Information("🎯 Selected track {TrackName} (ID: {TrackId}) for cup {CupId} (attempt {Attempt}/{MaxAttempts})", track.Name, track.TrackId, cupId, attempts, maxAttempts);
                resultsDto = await _velocidrone.LeaderboardAsync(track.TrackId);

                if (resultsDto.Count == 0)
                    Log.Warning("Track {TrackName} has no results, selecting another track", track.Name);

            } while (resultsDto.Count == 0 && attempts < maxAttempts);

            if (resultsDto.Count == 0)
                throw new InvalidOperationException($"No track with results found for cup {cupId} after {maxAttempts} attempts");

            quad ??= await _quadOfTheDayService.DetectQuadFromTrackNameAsync(track.Name, cupOptions);
            quad ??= await _quadOfTheDayService.GetQuadOfTheDayAsync(cupOptions, cupId);
        }

        var results = await _resultsConverter.ConvertTrackTimesAsync(resultsDto, cupOptions.QuadClasses, quad);
        Log.Debug("Retrieved {ResultCount} initial results from Velocidrone API for track {TrackId}", results.Count, track.TrackId);

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
        Log.Information("🚀 Competition {CompetitionId} started for cup {CupId} with {PilotCount} existing pilots on track {TrackName}", competition.Id, cupId, pilotsFlownOnTrack.Count, track.Name);

        await _mediator.Publish(new CompetitionStarted(competition, track, pilotsFlownOnTrack, cupOptions));

        Log.Information("Competition {CompetitionId} setup completed successfully for cup {CupId}", competition.Id, cupId);
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

    public async Task StopAsync(string cupId)
    {
        var competition = await GetActiveCompetitionAsync(cupId);

        if (competition is null)
        {
            Log.Warning("No active competition found in cup '{CupId}', skipping stop", cupId);
            return;
        }

        Log.Information("Stopping competition {CompetitionId} for track {TrackName} in cup {CupId}", competition.Id, competition.Track.Name, cupId);

        competition.State = CompetitionState.Closed;
        competition.CompetitionResults = _leaderboardCalculator.GetLeaderboard(competition);
        _quadOfTheDayService.PunishNonQuadOfTheDayPilots(competition);

        Log.Information("🏁 Competition {CompetitionId} stopped with {ResultCount} final results in cup {CupId}", competition.Id, competition.CompetitionResults.Count, cupId);

        await _competitions.SaveChangesAsync();

        var leaderboard = _leaderboardCalculator.GetLeagueLeaderboard(competition);
        await _mediator.Publish(new CompetitionFinished(competition, leaderboard));

        Log.Information("Competition {CompetitionId} closure process completed for cup {CupId}", competition.Id, cupId);
    }

    private async Task CancelAsync(string cupId)
    {
        var competition = await GetActiveCompetitionAsync(cupId);

        if (competition is null) throw new Exception($"There are no active competitions in cup {cupId}");

        Log.Information("Cancelling a competition {competitionId} in cup {CupId}", competition.Id, cupId);

        competition.State = CompetitionState.Cancelled;
        await _competitions.SaveChangesAsync();
        await _mediator.Publish(new CompetitionCancelled(competition));
    }

    public async Task StopPollAsync(string cupId)
    {
        Log.Debug("Stopping poll for cup {CupId}", cupId);

        var competition = await GetActiveCompetitionAsync(cupId);

        if (competition is null)
        {
            Log.Warning("No active competition found in cup '{CupId}', skipping poll stop", cupId);
            return;
        }

        if (competition.Track.Rating is null)
        {
            Log.Error("No poll to stop for competition {CompetitionId}", competition.Id);
            return;
        }

        Log.Information("Stopping Discord poll {PollId} for track {TrackName}", competition.Track.Rating.PollMessageId, competition.Track.FullName);
        var pollResults = await _discordCupMessenger.StopPollInCupAsync(cupId, competition.Track.Rating.PollMessageId);

        double? rating = null;

        if (pollResults is not null)
        {
            var totalVotes = pollResults.VoteCounts.Sum();

            if (totalVotes > 0)
            {
                var poll = _discordMessageComposer.Poll(competition.Track.FullName);

                var weightedSum = poll.Options
                    .Zip(pollResults.VoteCounts, (option, votes) => option.Points * votes)
                    .Sum();

                rating = (double)weightedSum / totalVotes;
            }
        }

        Log.Information("Discord poll {PollId} stopped for track {TrackName}, rating: {Rating}",
            competition.Track.Rating.PollMessageId, competition.Track.FullName, rating?.ToString("F2") ?? "no votes");

        competition.Track.Rating.Value = rating;
        await _competitions.SaveChangesAsync();

        if (rating is null or >= 3)
            return;

        Log.Warning("👎 Track {TrackName} received low rating {Rating:F2}/5, marking as bad track", competition.Track.FullName, rating);
        await _mediator.Publish(new BadTrack(competition, competition.Track));
    }

    public async Task SeasonResultsAsync(string cupId)
    {
        var now = DateTime.Now;
        Log.Information("Processing season results for {Date} (Day {Day} of month)", now.ToString("yyyy-MM-dd"), now.Day);

        if (now.Day == 1)
        {
            Log.Information("First day of month detected, stopping the season for cup {CupId}", cupId);
            await StopSeasonAsync(cupId);
        }
        else
        {
            Log.Debug("Publishing temporary season results for cup {CupId}", cupId);
            await TempSeasonResultsAsync(cupId);
        }
    }

    private async Task TempSeasonResultsAsync(string cupId)
    {
        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var results = await _leaderboardCalculator.GetSeasonLeaderboardAsync(cupId, firstDayOfMonth, today);
        var totalCount = results.Sum(l => l.Results.Count);

        Log.Debug("Retrieved {ResultCount} results for temporary season leaderboard for cup {CupId} ({StartDate} to {EndDate})",
            totalCount, cupId, firstDayOfMonth.ToString("yyyy-MM-dd"), today.ToString("yyyy-MM-dd"));

        if (totalCount == 0)
        {
            Log.Information("No results found for current season in cup {CupId}, skipping temporary results publication", cupId);
            return;
        }

        await _mediator.Publish(new TempSeasonResults(cupId, results));
        Log.Information("Published temporary season results for cup {CupId} with {ResultCount} entries", cupId, totalCount);
    }

    private async Task StopSeasonAsync(string cupId)
    {
        var today = DateTime.Now;
        var firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var seasonName = firstDayOfPreviousMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        Log.Information("Finalizing season {SeasonName} for cup {CupId} ({StartDate} to {EndDate})",
            seasonName, cupId, firstDayOfPreviousMonth.ToString("yyyy-MM-dd"), firstDayOfCurrentMonth.ToString("yyyy-MM-dd"));

        var results = await _leaderboardCalculator.GetSeasonLeaderboardAsync(cupId, firstDayOfPreviousMonth, firstDayOfCurrentMonth);
        var totalCount = results.Sum(l => l.Results.Count);

        if (totalCount == 0)
        {
            Log.Warning("No results found for season {SeasonName} in cup {CupId}, skipping season finalization", seasonName, cupId);
            return;
        }

        var winners = results
            .First().Results
            .Take(3)
            .Select(x => x.PlayerName)
            .ToArray();

        Log.Information("Season {SeasonName} for cup {CupId} completed with {ResultCount} participants. Winners: {Winners}",
            seasonName, cupId, totalCount, string.Join(", ", (IEnumerable<string>)winners));

        var image = await _imageService.CreateWinnerImageAsync(seasonName, winners);
        Log.Debug("Generated winner image for season {SeasonName} cup {CupId}", seasonName, cupId);

        await _mediator.Publish(new SeasonFinished(cupId, results, seasonName, winners, image, "winners.png"));
        Log.Information("Season {SeasonName} finalization completed for cup {CupId}", seasonName, cupId);
    }

    private async Task<Competition?> GetActiveCompetitionAsync(string cupId)
    {
        return await _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .ForCup(cupId)
            .FirstOrDefaultAsync();
    }
}
