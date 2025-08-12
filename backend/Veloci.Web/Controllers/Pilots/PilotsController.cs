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
    private readonly IPilotProfileService _pilotProfileService;

    public PilotsController(
        IRepository<Competition> competitions,
        IPilotProfileService pilotProfileService)
    {
        _competitions = competitions;
        _pilotProfileService = pilotProfileService;
    }

    [HttpGet]
    public async Task<List<string>> All()
    {
        var allPilots = await _competitions.GetAll()
            .NotCancelled()
            .SelectMany(comp => comp.CompetitionResults)
            .Select(res => res.PlayerName)
            .Distinct()
            .ToListAsync();

        return allPilots;
    }

    [HttpGet]
    public async Task<ActionResult<PilotProfileModel>> Profile(string pilotName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(pilotName))
        {
            return BadRequest("Pilot name is required");
        }

        var profile = await _pilotProfileService.GetPilotProfileAsync(pilotName, ct);

        return profile;
    }
}
