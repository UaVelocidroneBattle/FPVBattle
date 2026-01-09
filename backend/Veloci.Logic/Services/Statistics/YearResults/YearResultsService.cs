using MediatR;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Services.Statistics.YearResults;

public class YearResultsService
{
    private readonly IRepository<Competition> _competitions;
    private readonly CompetitionService _competitionService;
    private readonly IMediator _mediator;

    private readonly DateTime _from;
    private readonly DateTime _to;

    public YearResultsService(
        IRepository<Competition> competitions,
        IMediator mediator,
        CompetitionService competitionService)
    {
        _competitions = competitions;
        _mediator = mediator;
        _competitionService = competitionService;

        var today = DateTime.Today;
        var previousYear = today.Year - 1;
        _from = new DateTime(previousYear, 1, 1);
        _to = _from.AddYears(1);
    }

    public async Task Publish()
    {
        var statistics = new YearResultsModel
        {
            Year = _from.Year,
            PilotWithTheMostGoldenMedal = await GetPilotWithTheMostGoldenMedalAsync(),
            PilotWhoCameTheMost = await GetPilotWhoCameTheMostAsync(),
            PilotWhoCameTheLeast = await GetPilotWhoCameTheLeastAsync(),
            TracksSkipped = await GetTracksSkippedAsync(),
            TotalTrackCount = await GetTotalTrackCountAsync(),
            FavoriteTrack = await GetFavoriteTrackAsync(),
            UniqueTrackCount = await GetUniqueTrackCountAsync(),
            TotalPilotCount = await GetTotalPilotCountAsync(),
            Top3Pilots = await GetTop3PilotsAsync(),
        };

        await _mediator.Publish(new Notifications.YearResults(statistics));
    }

    private async Task<Dictionary<string, int>> GetTop3PilotsAsync()
    {
        return await _competitionService.GetSeasonResultsQuery(_from, _to)
            .OrderByDescending(r => r.Points)
            .Take(3)
            .ToDictionaryAsync(x => x.PlayerName, x => x.Points);
    }

    private async Task<(string name, int count)> GetPilotWithTheMostGoldenMedalAsync()
    {
        var result = await _competitionService.GetSeasonResultsQuery(_from, _to)
            .OrderByDescending(result => result.GoldenCount)
            .FirstOrDefaultAsync();

        if (result is null)
            throw new Exception("No results found");

        var count = result.GoldenCount;
        var name = result.PlayerName;

        return (name, count);
    }

    private async Task<int> GetTotalTrackCountAsync()
    {
        return await _competitions
            .GetAll()
            .InRange(_from, _to)
            .NotCancelled()
            .CountAsync();
    }

    private async Task<int> GetUniqueTrackCountAsync()
    {
        return await _competitions
            .GetAll()
            .InRange(_from, _to)
            .NotCancelled()
            .Select(comp => comp.TrackId)
            .Distinct()
            .CountAsync();
    }

    private async Task<int> GetTracksSkippedAsync()
    {
        return await _competitions
            .GetAll()
            .InRange(_from, _to)
            .Where(comp => comp.State == CompetitionState.Cancelled)
            .CountAsync();
    }

    private async Task<(string name, int count)> GetPilotWhoCameTheLeastAsync()
    {
        var result = await _competitions
            .GetAll()
            .InRange(_from, _to)
            .NotCancelled()
            .SelectMany(comp => comp.CompetitionResults)
            .GroupBy(res => res.Pilot.Id)
            .Select(group => new
            {
                Name = group.First().Pilot.Name,
                Count = group.Count()
            })
            .OrderByDescending(x => x.Count)
            .LastOrDefaultAsync();

        if (result is null)
            throw new Exception("No results found");

        return (result.Name, result.Count);
    }

    private async Task<(string name, int count)> GetPilotWhoCameTheMostAsync()
    {
        var result = await _competitions
            .GetAll()
            .InRange(_from, _to)
            .NotCancelled()
            .SelectMany(comp => comp.CompetitionResults)
            .GroupBy(res => res.Pilot.Name)
            .Select(group => new
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        if (result is null)
            throw new Exception("No results found");

        return (result.Name, result.Count);
    }

    private async Task<string> GetFavoriteTrackAsync()
    {
        var favoriteMap = await _competitions
            .GetAll()
            .InRange(_from, _to)
            .NotCancelled()
            .Select(comp => new
            {
                Name = comp.Track.FullName,
                Rating = comp.Track.Rating != null ? comp.Track.Rating.Value : 0
            })
            .OrderByDescending(m => m.Rating)
            .FirstOrDefaultAsync();

        return favoriteMap?.Name ?? "No favorite track";
    }

    private async Task<int> GetTotalPilotCountAsync()
    {
        return await _competitions
            .GetAll()
            .InRange(_from, _to)
            .NotCancelled()
            .SelectMany(comp => comp.CompetitionResults)
            .Select(res => res.PilotId)
            .Distinct()
            .CountAsync();
    }
}
