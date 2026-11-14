using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Logic.Services.Pilots;
using FpvBattle.Logic.Services.Pilots.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Web.Controllers.Api.Pilots;

[ApiController]
[Route("/api/pilots/[action]")]
public class PilotsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly IPilotProfileService _pilotProfileService;

    public PilotsController(
        IApplicationDbContext db,
        IPilotProfileService pilotProfileService)
    {
        _db = db;
        _pilotProfileService = pilotProfileService;
    }

    [HttpGet]
    public async Task<List<string>> All()
    {
        return await _db.Pilots
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

        if (profile is null)
        {
            return NotFound($"Pilot '{pilotName}' was not found");
        }

        return profile;
    }
}
