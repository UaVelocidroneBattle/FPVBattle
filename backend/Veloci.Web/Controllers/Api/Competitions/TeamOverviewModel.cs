using Veloci.Logic.Features.Teams.Leaderboard;

namespace Veloci.Web.Controllers.Api.Competitions;

public class TeamOverviewModel
{
    public required List<TeamLeaderboardResultModel> Leaderboard { get; set; }
    public required List<TeamSeasonResultModel> SeasonLeaderboard { get; set; }
}

public class TeamLeaderboardResultModel
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }
    public string? TeamTag { get; set; }
    public bool IsDeleted { get; set; }
    public int AverageTime { get; set; }
    public int Rank { get; set; }
    public int Points { get; set; }
    public required List<TeamPilotTimeModel> Pilots { get; set; }
}

public class TeamPilotTimeModel
{
    public required string PilotName { get; set; }
    public required string Country { get; set; }
    public int TrackTime { get; set; }
}

public class TeamSeasonResultModel
{
    public int TeamId { get; set; }
    public required string TeamName { get; set; }
    public string? TeamTag { get; set; }
    public bool IsDeleted { get; set; }
    public int Points { get; set; }
    public int Rank { get; set; }
}

public static class TeamLeaderboardMapper
{
    public static TeamLeaderboardResultModel ToModel(TeamDailyStanding standing) => new()
    {
        TeamId = standing.Team.Id,
        TeamName = standing.Team.Name,
        TeamTag = standing.Team.Tag,
        IsDeleted = standing.Team.IsDeleted,
        AverageTime = standing.AverageTime,
        Rank = standing.Rank,
        Points = standing.Points,
        Pilots = standing.Pilots
            .Select(p => new TeamPilotTimeModel { PilotName = p.Pilot.Name, Country = p.Pilot.Country, TrackTime = p.TrackTime })
            .ToList()
    };

    public static TeamSeasonResultModel ToModel(TeamSeasonStanding standing) => new()
    {
        TeamId = standing.Team.Id,
        TeamName = standing.Team.Name,
        TeamTag = standing.Team.Tag,
        IsDeleted = standing.Team.IsDeleted,
        Points = standing.Points,
        Rank = standing.Rank
    };
}
