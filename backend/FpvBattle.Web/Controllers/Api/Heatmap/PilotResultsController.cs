using Microsoft.AspNetCore.Mvc;
using FpvBattle.Data.Repositories;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Web.Controllers.Api.Heatmap;

[ApiController]
[Route("/api/results/[action]")]
public class PilotResultsController : ControllerBase
{
    private readonly PilotResultsCalculator _calculator;
    private readonly IRepository<Pilot> _pilots;

    public PilotResultsController(PilotResultsCalculator calculator, IRepository<Pilot> pilots)
    {
        _calculator = calculator;
        _pilots = pilots;
    }

    [HttpGet]
    public async Task<ActionResult<List<PilotResult>>> ForPilot([FromQuery]string pilotName, CancellationToken ct)
    {
        var pilot = await _pilots.GetAll().ByName(pilotName).FirstOrDefaultAsync(cancellationToken: ct);

        if (pilot is null)
        {
            return NotFound($"Pilot '{pilotName}' was not found");
        }

        return await _calculator.GetPilotResults(pilot, ct);
    }
}
