using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Services.Statistics;

public class StatisticsService
{
    private static readonly ILogger _log = Log.ForContext<StatisticsService>();
    private readonly IRepository<Competition> _competitions;
    private readonly IMediator _mediator;

    public StatisticsService(IRepository<Competition> competitions, IMediator mediator)
    {
        _competitions = competitions;
        _mediator = mediator;
    }

    [Obsolete("Need to rework")]
    public async Task PublishEndOfSeasonStatisticsAsync()
    {
        var statistics = await EndOfSeasonStatisticsAsync();
        await _mediator.Publish(new EndOfSeasonStatisticsNotification(statistics));
        _log.Information("End of season statistics published");
    }

    private async Task<EndOfSeasonStatisticsDto> EndOfSeasonStatisticsAsync()
    {
        _log.Debug("Calculating end of season statistics");

        var today = DateTime.Now;
        var firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfMonthYearAgo = firstDayOfPreviousMonth.AddYears(-1);
        var seasonName = firstDayOfPreviousMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        var averagePilotsForMonth = await CalculateAveragePilotsPerDayAsync(firstDayOfPreviousMonth, firstDayOfCurrentMonth);
        var averagePilotsForYear = await CalculateAveragePilotsPerDayAsync(firstDayOfMonthYearAgo, firstDayOfCurrentMonth);
        var minPilotsForMonth = CalculateMinPilotsForPeriod(firstDayOfPreviousMonth, firstDayOfCurrentMonth);
        var maxPilotsForMonth = CalculateMaxPilotsForPeriod(firstDayOfPreviousMonth, firstDayOfCurrentMonth);

        return new EndOfSeasonStatisticsDto
        {
            SeasonName = seasonName,
            AveragePilotsLastMonth = averagePilotsForMonth,
            AveragePilotsLastYear = averagePilotsForYear,
            MinPilotsLastMonth = minPilotsForMonth,
            MaxPilotsLastMonth = maxPilotsForMonth,
        };
    }

    private async Task<double> CalculateAveragePilotsPerDayAsync(DateTime from, DateTime to)
    {
        var average = await CompetitionsQuery(from, to)
            .Select(c => c.CompetitionResults.Count)
            .AverageAsync();

        return Math.Round(average * 10) / 10;
    }

    private int CalculateMinPilotsForPeriod(DateTime from, DateTime to)
    {
        return CompetitionsQuery(from, to)
            .Select(c => c.CompetitionResults.Count)
            .Min();
    }

    private int CalculateMaxPilotsForPeriod(DateTime from, DateTime to)
    {
        return CompetitionsQuery(from, to)
            .Select(c => c.CompetitionResults.Count)
            .Max();
    }

    private IQueryable<Competition> CompetitionsQuery(DateTime from, DateTime to)
    {
        return _competitions
            .GetAll()
            .InRange(from, to)
            .NotCancelled();
    }
}
