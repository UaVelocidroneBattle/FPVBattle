namespace Veloci.Logic.Features.Leagues.Models;

public class LeagueUpdateModel
{
    public required string PilotName { get; set; }
    public string? OldLeague { get; set; }
    public string? NewLeague { get; set; }
}
