namespace Veloci.Logic.Services.Pilots.Models;

public class PilotAchievementModel
{
    public required string Name { get; set; }
    public required DateTime? AchievedOn { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
}
