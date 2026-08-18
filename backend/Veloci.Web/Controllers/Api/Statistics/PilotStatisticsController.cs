using Microsoft.AspNetCore.Mvc;
using Veloci.Logic.Services.Statistics;

namespace Veloci.Web.Controllers.Api.Statistics;

/// <summary>
/// Community-wide pilot numbers for the statistics dashboard.
/// </summary>
/// <remarks>
/// Each chart asks only for the range it is showing, so switching to a wider scale
/// costs one small query instead of the dashboard carrying years of history it may
/// never draw. Omitting the range parameter means the whole history.
/// </remarks>
[ApiController]
[Route("/api/pilot-statistics/[action]")]
public class PilotStatisticsController : ControllerBase
{
    private const string DayFormat = "yyyy-MM-dd";
    private const string MonthFormat = "yyyy-MM";

    private readonly PilotsCountStatisticsService _pilotsCount;
    private readonly ParticipationStatisticsService _participation;

    public PilotStatisticsController(
        PilotsCountStatisticsService pilotsCount,
        ParticipationStatisticsService participation)
    {
        _pilotsCount = pilotsCount;
        _participation = participation;
    }

    [HttpGet]
    public async Task<PilotsCountModel> Count(int? days)
    {
        var statistics = await _pilotsCount.GetPilotsCountStatistics(days);

        return new PilotsCountModel(
            Days: statistics.Days.Select(day => day.ToString(DayFormat)).ToList(),
            Totals: statistics.Totals);
    }

    [HttpGet]
    public async Task<NewPilotsCountModel> NewPilots(int? months)
    {
        var statistics = await _pilotsCount.GetNewPilotsCountStatistics(months);

        return new NewPilotsCountModel(
            Months: statistics.Months.Select(month => month.ToString(MonthFormat)).ToList(),
            Counts: statistics.Counts);
    }

    [HttpGet]
    public async Task<ParticipationModel> Participation(int? days)
    {
        var statistics = await _participation.GetDailyParticipationAsync(days);

        return new ParticipationModel(
            Days: statistics.Days.Select(day => day.ToString(DayFormat)).ToList(),
            Series: statistics.Series
                .Select(cup => new CupParticipationModel(cup.CupId, cup.CupName, cup.Counts))
                .ToList());
    }
}
