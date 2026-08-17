using Microsoft.AspNetCore.Mvc;
using Veloci.Logic.Features.Cups;

namespace Veloci.Web.Controllers.Api.Cups;

[ApiController]
[Route("/api/cups/[action]")]
public class CupsController : ControllerBase
{
    private readonly ICupService _cupService;

    public CupsController(ICupService cupService)
    {
        _cupService = cupService;
    }

    /// <summary>
    /// The enabled cups, in configuration order. The dashboard builds its cup
    /// menus and routes from this list.
    /// </summary>
    [HttpGet("/api/cups/get")]
    public CupModel[] GetCups() =>
        _cupService.GetAllCups()
            .Where(cup => cup.Value.IsEnabled)
            .Select(cup => new CupModel { Id = cup.Key, Name = cup.Value.Name })
            .ToArray();
}
