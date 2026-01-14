using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Competitions;

public class CompetitionModel
{
    public required string Id { get; set; }
    public required DateTime StartedOn { get; set; }
    public required CompetitionState State { get; set; }
    public required string MapName { get; set; }
    public required string TrackName { get; set; }
    public required int TrackId { get; set; }
    public required int MapId { get; set; }
}

public class DashboardModel
{
    public CompetitionModel? Competition { get; set; }
    public required List<TrackTimeModel> Results { get; set; }
    public required List<SeasonResultModel> Leaderboard { get; set; }
}

public class TrackTimeModel
{
    public required string PlayerName { get; set; }
    public required int Time { get; set; }
    public required int GlobalRank { get; set; }
    public required int LocalRank { get; set; }
    public required string ModelName { get; set; }
}

public class SeasonResultModel
{
    public required string PlayerName { get; set; }
    public required int Points { get; set; }
}
