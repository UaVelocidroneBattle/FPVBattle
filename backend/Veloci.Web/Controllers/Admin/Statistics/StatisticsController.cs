using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Veloci.Logic.Services.Statistics;

namespace Veloci.Web.Controllers.Admin.Statistics;

public class StatisticsController : AdminControllerBase
{
    private static readonly JsonSerializerOptions ChartJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ParticipationStatisticsService _participation;

    public StatisticsController(ParticipationStatisticsService participation)
    {
        _participation = participation;
    }

    public async Task<IActionResult> Index(string? range)
    {
        var selected = ParticipationRange.Resolve(range);
        var chart = await GetChartAsync(selected);

        return View(new ParticipationViewModel
        {
            Ranges = ParticipationRange.All,
            SelectedRange = selected,
            ChartJson = JsonSerializer.Serialize(chart, ChartJsonOptions)
        });
    }

    /// <summary>
    /// Feeds the chart when the reader switches range, so a new scale costs one small
    /// query instead of a full page load.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Participation(string? range)
    {
        return Json(await GetChartAsync(ParticipationRange.Resolve(range)));
    }

    private async Task<ParticipationChart> GetChartAsync(ParticipationRange range)
    {
        var participation = await _participation.GetDailyParticipationAsync(range.Days);

        return new ParticipationChart(
            Days: participation.Days.Select(day => day.ToString("yyyy-MM-dd")).ToList(),
            Series: participation.Series
                .Select(cup => new CupSeries(Name: cup.CupName, Counts: cup.Counts))
                .ToList());
    }
}
