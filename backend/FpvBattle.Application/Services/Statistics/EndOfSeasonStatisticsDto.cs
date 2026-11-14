namespace FpvBattle.Application.Services.Statistics;

public class EndOfSeasonStatisticsDto
{
    public string SeasonName { get; set; }
    public double AveragePilotsLastMonth { get; set; }
    public double AveragePilotsLastYear { get; set; }
    public int MinPilotsLastMonth { get; set; }
    public int MaxPilotsLastMonth { get; set; }
}
