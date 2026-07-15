using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Helpers;

namespace Veloci.Web.Controllers.Landing;

[ApiController]
[Route("/api/landing/[action]")]
public class LandingController : ControllerBase
{
    private const string CacheKey = "landing-data";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<CompetitionResults> _competitionResults;
    private readonly IMemoryCache _cache;

    public LandingController(
        IRepository<Pilot> pilots,
        IRepository<CompetitionResults> competitionResults,
        IMemoryCache cache)
    {
        _pilots = pilots;
        _competitionResults = competitionResults;
        _cache = cache;
    }

    [HttpGet("/api/landing/get")]
    public async Task<LandingDataModel> GetData()
    {
        var data = await _cache.GetOrCreateAsync(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return BuildDataAsync();
        });

        return data!;
    }

    private async Task<LandingDataModel> BuildDataAsync()
    {
        var countryPilots = await GetCountryPilotsAsync();

        return new LandingDataModel
        {
            TotalPilots = await _pilots.GetAll().CountAsync(),
            TotalCountries = countryPilots.Count,
            DailyActivePilots = await GetDailyActivePilotsAsync(),
            CountryPilots = countryPilots
        };
    }

    /// <summary>
    /// Average number of distinct pilots per competition day over the last 30 days.
    /// </summary>
    private async Task<int> GetDailyActivePilotsAsync()
    {
        const int watchInPastDays = 30;
        var today = DateTime.Today;
        var startFrom = today.AddDays(-watchInPastDays);

        var dailyPilotCounts = await _competitionResults
            .GetAll(r => r.Competition.State == CompetitionState.Closed)
            .Where(r => r.Competition.StartedOn >= startFrom)
            .Select(r => new { Day = r.Competition.StartedOn.Date, r.PilotId })
            .Distinct()
            .GroupBy(x => x.Day)
            .Select(g => g.Count())
            .ToListAsync();

        return dailyPilotCounts.Count == 0
            ? 0
            : (int)Math.Round(dailyPilotCounts.Average());
    }

    private async Task<List<CountryPilotsModel>> GetCountryPilotsAsync()
    {
        var pilotsByCountry = await _pilots
            .GetAll(p => !string.IsNullOrEmpty(p.Country))
            .GroupBy(p => p.Country)
            .Select(g => new { CountryCode = g.Key, Pilots = g.Count() })
            .OrderByDescending(g => g.Pilots)
            .ToListAsync();

        return pilotsByCountry
            .Select(c => new CountryPilotsModel
            {
                Country = TextHelper.CountryName(c.CountryCode),
                CountryCode = c.CountryCode,
                Pilots = c.Pilots
            })
            .ToList();
    }
}
