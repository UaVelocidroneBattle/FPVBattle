using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Web.Controllers.Pilots;

[ApiController]
[Route("/api/pilots/[action]")]
public class PilotsController : ControllerBase
{
    private readonly IRepository<Competition> _competitions;

    public PilotsController(IRepository<Competition> competitions)
    {
        _competitions = competitions;
    }

    [HttpGet]
    public async Task<List<string>> All()
    {
        var allPilots = await _competitions.GetAll()
            .NotCancelled()
            .SelectMany(comp => comp.CompetitionResults)
            .Select(res => res.Pilot.Name)
            .Distinct()
            .ToListAsync();

        return allPilots;
    }
}
