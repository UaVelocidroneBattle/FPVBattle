using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;

namespace Veloci.Web.Controllers.Competitions;

public class DashboardModel
{
    public CompetitionModel? Competition { get; set; }
    public required List<LeagueLeaderboardModel> Leaderboard { get; set; }
    public required List<LeagueSeasonLeaderboard> SeasonLeaderboard { get; set; }
}

public class LeagueLeaderboardModel
{
    public string? League { get; set; }
    public required List<LeaderboardResultModel> Results { get; set; }
}

public class LeaderboardResultModel
{
    public required string PlayerName { get; set; }
    public int TrackTime { get; set; }
    public int LocalRank { get; set; }
    public int GlobalRank { get; set; }
    public string? ModelName { get; set; }
    public required string Country { get; set; }
}

public class CompetitionModel
{
    public required string Id { get; set; }
    public required DateTime StartedOn { get; set; }
    public required CompetitionState State { get; set; }
    public required string MapName { get; set; }
    public required string TrackName { get; set; }
    public required int TrackId { get; set; }
    public required int MapId { get; set; }
    public string? QuadOfTheDay { get; set; }
    public LeagueOptions Leagues { get; set; } = new();
}
