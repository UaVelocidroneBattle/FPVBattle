using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Leagues.Models;

public class LeagueUpdateModel
{
    public required string CupId { get; set; }
    public required Pilot Pilot { get; set; }
    public string? OldLeague { get; set; }
    public string? NewLeague { get; set; }
}
