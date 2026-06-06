namespace Veloci.Web.Controllers.DayStreak;

public class DayStreakLeaderboardRow
{
    public required string PilotName { get; set; }
    public int DayStreak  { get; set; }
    public int MaxStreak { get; set; }
}
