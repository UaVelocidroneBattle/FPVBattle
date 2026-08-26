using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Leagues.Models;

public class LeagueUpdateModel
{
    public required Pilot Pilot { get; set; }
    public string? OldLeague { get; set; }
    public string? NewLeague { get; set; }
}
