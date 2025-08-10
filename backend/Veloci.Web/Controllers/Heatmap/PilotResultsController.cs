using Microsoft.AspNetCore.Mvc;

namespace Veloci.Web.Controllers.Heatmap;

[ApiController]
[Route("/api/results/[action]")]
public class PilotResultsController : ControllerBase
{
    private readonly PilotResultsCalculator _calculator;

    public PilotResultsController(PilotResultsCalculator calculator)
    {
        _calculator = calculator;
    }

    [HttpGet]
    public async Task<List<PilotResult>> ForPilot([FromQuery]string pilotName, CancellationToken ct)
    {
        var data = await _calculator.GetPilotResults(pilotName, ct);
        return data;
    }
}
