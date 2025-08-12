namespace Veloci.Web.Controllers.Pilots;

public class PilotAchievementModel
{
    public required string Name { get; set; }
    public required DateTime EarnedOn { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
}
