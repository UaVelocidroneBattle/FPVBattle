namespace Veloci.Logic.Services.Pilots.Models;

public class PilotAchievementModel
{
    public required string Name { get; set; }
    public required DateTime EarnedOn { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
}
