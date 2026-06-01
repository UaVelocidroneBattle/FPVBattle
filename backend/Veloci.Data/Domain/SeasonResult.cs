namespace Veloci.Data.Domain;

public class SeasonResult
{
    public required string PlayerName { get; set; }
    public int Points { get; set; }
    public int Rank { get; set; }
    public required string Country { get; set; }
}

public class LeagueSeasonLeaderboard
{
    public string? League { get; set; }
    public required List<SeasonResult> Results { get; set; }
}
