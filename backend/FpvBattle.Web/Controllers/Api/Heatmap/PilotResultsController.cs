using FpvBattle.Domain;
using FpvBattle.Application.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FpvBattle.Application.Persistence.Queries;

namespace FpvBattle.Web.Controllers.Api.Heatmap;

[ApiController]
[Route("/api/results/[action]")]
public class PilotResultsController : ControllerBase
{
    private readonly PilotResultsCalculator _calculator;
    private readonly IApplicationDbContext _db;

    public PilotResultsController(PilotResultsCalculator calculator, IApplicationDbContext db)
    {
        _calculator = calculator;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PilotResult>>> ForPilot([FromQuery]string pilotName, CancellationToken ct)
    {
        var pilot = await _db.Pilots.ByName(pilotName).FirstOrDefaultAsync(cancellationToken: ct);

        if (pilot is null)
        {
            return NotFound($"Pilot '{pilotName}' was not found");
        }

        return await _calculator.GetPilotResults(pilot, ct);
    }
}
