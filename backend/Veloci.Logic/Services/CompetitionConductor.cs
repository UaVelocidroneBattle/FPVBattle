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
        Log.Information("Starting a new competition");

        var activeComp = await GetActiveCompetitionAsync();

        if (activeComp is not null)
        {
            await StopPollAsync();
            await CancelAsync();
        }

        var track = await _trackService.GetRandomTrackAsync();
        var resultsDto = await _velocidrone.LeaderboardAsync(track.TrackId);
        var results = _resultsConverter.ConvertTrackTimes(resultsDto);
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

        await _mediator.Publish(new CompetitionStarted(competition, track));

        //possible needs to be moved to CompetitionStarted event handler in TelegramHandler
        await CreatePoll(track, competition);

        await _competitions.SaveChangesAsync();
    }

    private async Task CreatePoll(Track track, Competition competition)
    {
        var poll = _messageComposer.Poll(track.FullName);
        var pollId = await TelegramBot.SendPollAsync(poll);

        if (pollId is null) return;

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

        Log.Information("Stopping a competition {competitionId}", competition.Id);

        competition.State = CompetitionState.Closed;
        competition.CompetitionResults = _competitionService.GetLocalLeaderboard(competition);

        await UpdateDayStreakAsync(competition.CompetitionResults);
        await _competitions.SaveChangesAsync();

        await _mediator.Publish(new CompetitionStopped(competition));
    }

    private async Task UpdateDayStreakAsync(List<CompetitionResults> competitionResults)
    {
        var today = DateTime.Today;

        foreach (var results in competitionResults)
        {
            var pilot = await _pilots.GetAll()
                .FirstOrDefaultAsync(p => p.Name == results.PlayerName);

            if (pilot is null)
            {
                pilot = new Pilot(results.PlayerName);
                await _pilots.AddAsync(pilot);
            }

            pilot.IncreaseDayStreak(today);
        }

        await _pilots.SaveChangesAsync();
        await _pilots.GetAll().ResetDayStreaksAsync(today);
    }

    private async Task CancelAsync()
    {
        var competition = await GetActiveCompetitionAsync();

        if (competition is null) throw new Exception("There are no active competitions");

        Log.Information("Cancelling a competition {competitionId}", competition.Id);

        competition.State = CompetitionState.Cancelled;
        await _competitions.SaveChangesAsync();
    }

    public async Task StopPollAsync()
    {
        Log.Debug("Stopping poll");

        var competition = await GetActiveCompetitionAsync();

        if (competition is null)
            throw new Exception("There are no active competitions");

        var poll = _messageComposer.Poll(competition.Track.FullName);

        if (competition.Track.Rating is null)
        {
            Log.Error("No poll to stop");
            return;
        }

        var telegramPoll = await TelegramBot.StopPollAsync(competition.Track.Rating.PollMessageId);

        if (telegramPoll is null)
        {
            Log.Error("Poll is already stopped");
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

        competition.Track.Rating.Value = rating;
        await _competitions.SaveChangesAsync();

        if (rating is null or >= 0)
            return;

        await _mediator.Publish(new BadTrack(competition, competition.Track));
    }

    public async Task SeasonResultsAsync()
    {
        Log.Debug("Publishing season results");

        var now = DateTime.Now;

        if (now.Day == 1)
        {
            await StopSeasonAsync();
        }
        else
        {
            await TempSeasonResultsAsync();
        }
    }

    public async Task VoteReminder()
    {
        Log.Debug("Publishing vote reminder");

        var competition = await GetActiveCompetitionAsync();
        var messageText = ChatMessages.GetRandomByType(ChatMessageType.VoteReminder);

        await TelegramBot.ReplyMessageAsync(messageText.Text, competition.Track.Rating.PollMessageId);
    }

    private async Task TempSeasonResultsAsync()
    {
        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var results = await _competitionService.GetSeasonResultsAsync(firstDayOfMonth, today);

        if (results.Count == 0)
            return;

        await _mediator.Publish(new TempSeasonResults(results));
    }

    private async Task StopSeasonAsync()
    {
        var today = DateTime.Now;
        var firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);

        var results = await _competitionService.GetSeasonResultsAsync(firstDayOfPreviousMonth, firstDayOfCurrentMonth);

        if (results.Count == 0) return;

        var seasonName = firstDayOfPreviousMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        var winnerName = results.FirstOrDefault().PlayerName;

        var image = await _imageService.CreateWinnerImageAsync(seasonName, winnerName);

        await _mediator.Publish(new SeasonFinished(results, seasonName, winnerName, image));
    }

    private async Task<Competition?> GetActiveCompetitionAsync()
    {
        return await _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .FirstOrDefaultAsync();
    }
}
