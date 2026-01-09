namespace Veloci.Logic.Services.Pilots.Models;

public class PilotProfileModel
{
    public required string Name { get; set; }
    public required int CurrentDayStreak { get; set; }
    public required int MaxDayStreak { get; set; }
    public required List<PilotAchievementModel> Achievements { get; set; }
    public required int TotalRaceDays { get; set; }
    public DateTime? LastRaceDate { get; set; }
    public DateTime? FirstRaceDate { get; set; }
    public required int AvailableFreezes { get; set; }
}
