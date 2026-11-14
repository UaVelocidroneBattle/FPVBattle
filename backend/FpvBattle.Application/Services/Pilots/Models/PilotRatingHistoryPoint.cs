namespace FpvBattle.Application.Services.Pilots.Models;

public class PilotRatingHistoryPoint
{
    public DateTime Date { get; set; }
    public double? GapPercent { get; set; }
    public int Rank { get; set; }
}