using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Web.Controllers.Heatmap;

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
    public async Task<List<PilotResult>> ForPilot([FromQuery]string pilotName, CancellationToken ct)
    {
        var pilot = await _pilots.GetAll().ByName(pilotName).FirstAsync(cancellationToken: ct);
        var data = await _calculator.GetPilotResults(pilot, ct);
        return data;
    }
}
