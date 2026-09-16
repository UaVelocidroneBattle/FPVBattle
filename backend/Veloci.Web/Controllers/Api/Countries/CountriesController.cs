using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Helpers;

namespace Veloci.Web.Controllers.Api.Countries;

[ApiController]
[Route("/api/countries/[action]")]
public class CountriesController : ControllerBase
{
    private readonly IRepository<Pilot> _pilots;

    public CountriesController(IRepository<Pilot> pilots)
    {
        _pilots = pilots;
    }

    [HttpGet]
    public async Task<List<CountryModel>> All()
    {
        var pilotsByCountry = await _pilots
            .GetAll()
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
        return await _pilots
            .GetAll(p => p.Country == countryCode)
            .OrderBy(p => p.Name)
            .Select(p => new CountryPilotModel
            {
                PilotId = p.Id,
                PilotName = p.Name
            })
            .ToListAsync();
    }
}
