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
        return await _pilots
            .GetAll()
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .ToListAsync();
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
