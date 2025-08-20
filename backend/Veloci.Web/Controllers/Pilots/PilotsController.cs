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
    private readonly IRepository<Pilot> _pilots;

    public PilotsController(
        IRepository<Competition> competitions,
        IPilotProfileService pilotProfileService,
        IRepository<Pilot> pilots)
    {
        _competitions = competitions;
        _pilotProfileService = pilotProfileService;
        _pilots = pilots;
    }

    [HttpGet]
    public async Task<List<string>> All()
    {
        var competitionResults = from comp in _competitions.GetAll().NotCancelled()
            from res in comp.CompetitionResults
            where res.UserId.HasValue
            select res;

        var allPilotNames =
            from pilot in _pilots.GetAll()
            join result in competitionResults on pilot.Id equals result.UserId.Value
            select pilot.Name;

        return await allPilotNames.Distinct().ToListAsync();
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
