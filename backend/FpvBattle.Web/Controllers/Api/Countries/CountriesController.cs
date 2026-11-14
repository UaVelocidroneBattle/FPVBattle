using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Logic.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Web.Controllers.Api.Countries;

[ApiController]
[Route("/api/countries/[action]")]
public class CountriesController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public CountriesController(IApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<List<CountryModel>> All()
    {
        var pilotsByCountry = await _db.Pilots
            .GroupedByCountry()
            .Select(g => new { CountryCode = g.Key, PilotsCount = g.Count() })
            .OrderByDescending(g => g.PilotsCount)
            .ToListAsync();

        return pilotsByCountry
            .Select(c => new CountryModel
            {
                CountryCode = c.CountryCode,
                CountryName = TextHelper.CountryName(c.CountryCode),
                PilotsCount = c.PilotsCount
            })
            .ToList();
    }

    [HttpGet]
    public async Task<List<CountryPilotModel>> Pilots(string countryCode)
    {
        return await _db.Pilots.Where(p => p.Country == countryCode)
            .OrderBy(p => p.Name)
            .Select(p => new CountryPilotModel
            {
                PilotId = p.Id,
                PilotName = p.Name
            })
            .ToListAsync();
    }
}
