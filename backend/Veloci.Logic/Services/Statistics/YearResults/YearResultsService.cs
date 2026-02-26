using MediatR;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Services.Statistics.YearResults;

public class YearResultsService
{
    private readonly IRepository<Competition> _competitions;
    private readonly CompetitionService _competitionService;
    private readonly IMediator _mediator;
    private readonly ICupService _cupService;

    private readonly DateTime _from;
    private readonly DateTime _to;

    public YearResultsService(
        IRepository<Competition> competitions,
        IMediator mediator,
        CompetitionService competitionService,
        ICupService cupService)
    {
        _competitions = competitions;
        _mediator = mediator;
        _competitionService = competitionService;
        _cupService = cupService;

        var today = DateTime.Today;
        var previousYear = today.Year - 1;
        _from = new DateTime(previousYear, 1, 1);
        _to = _from.AddYears(1);
    }

    public async Task Publish()
    {
        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();

        foreach (var cupId in enabledCupIds)
        {
            var statistics = new YearResultsModel
            {
                Year = _from.Year,
                PilotWithTheMostGoldenMedal = await GetPilotWithTheMostGoldenMedalAsync(cupId),
                PilotWhoCameTheMost = await GetPilotWhoCameTheMostAsync(cupId),
                PilotWhoCameTheLeast = await GetPilotWhoCameTheLeastAsync(cupId),
                TracksSkipped = await GetTracksSkippedAsync(cupId),
                TotalTrackCount = await GetTotalTrackCountAsync(cupId),
                FavoriteTrack = await GetFavoriteTrackAsync(cupId),
                UniqueTrackCount = await GetUniqueTrackCountAsync(cupId),
                TotalPilotCount = await GetTotalPilotCountAsync(cupId),
                Top3Pilots = await GetTop3PilotsAsync(cupId),
            };

            await _mediator.Publish(new Notifications.YearResults(statistics));
        }
    }

    private async Task<Dictionary<string, int>> GetTop3PilotsAsync(string cupId)
    {
        return await _competitionService.GetSeasonResultsQuery(cupId, _from, _to)
            .OrderByDescending(r => r.Points)
            .Take(3)
            .ToDictionaryAsync(x => x.PlayerName, x => x.Points);
    }

    private async Task<(string name, int count)> GetPilotWithTheMostGoldenMedalAsync(string cupId)
    {
        var result = await _competitionService.GetSeasonResultsQuery(cupId, _from, _to)
            .OrderByDescending(result => result.GoldenCount)
            .FirstOrDefaultAsync();

        if (result is null)
            throw new Exception("No results found");

        var count = result.GoldenCount;
        var name = result.PlayerName;

        return (name, count);
    }

    private async Task<int> GetTotalTrackCountAsync(string cupId)
    {
        return await _competitions
            .GetAll()
            .ForCup(cupId)
            .InRange(_from, _to)
            .NotCancelled()
            .CountAsync();
    }

    private async Task<int> GetUniqueTrackCountAsync(string cupId)
    {
        return await _competitions
            .GetAll()
            .ForCup(cupId)
            .InRange(_from, _to)
            .NotCancelled()
            .Select(comp => comp.TrackId)
            .Distinct()
            .CountAsync();
    }

    private async Task<int> GetTracksSkippedAsync(string cupId)
    {
        return await _competitions
            .GetAll()
            .ForCup(cupId)
            .InRange(_from, _to)
            .Where(comp => comp.State == CompetitionState.Cancelled)
            .CountAsync();
    }

    private async Task<(string name, int count)> GetPilotWhoCameTheLeastAsync(string cupId)
    {
        var result = await _competitions
            .GetAll()
            .ForCup(cupId)
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

    private async Task<(string name, int count)> GetPilotWhoCameTheMostAsync(string cupId)
    {
        var result = await _competitions
            .GetAll()
            .ForCup(cupId)
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

    private async Task<string> GetFavoriteTrackAsync(string cupId)
    {
        var favoriteMap = await _competitions
            .GetAll()
            .ForCup(cupId)
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

    private async Task<int> GetTotalPilotCountAsync(string cupId)
    {
        return await _competitions
            .GetAll()
            .ForCup(cupId)
            .InRange(_from, _to)
            .NotCancelled()
            .SelectMany(comp => comp.CompetitionResults)
            .Select(res => res.PilotId)
            .Distinct()
            .CountAsync();
    }
}
