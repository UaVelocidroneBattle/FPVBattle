namespace Veloci.Web.Controllers.Rating;

public class RatingModel
{
    public DateTime CalculatedOn { get; set; }
    public IList<PilotRatingModel> Ratings { get; set; }
    public IList<PilotRatingModel> DroppedOutPilots { get; set; } = [];
    public LeagueSettingsModel LeagueSettings { get; set; } = new();
}

public class PilotRatingModel
{
    public int PilotId { get; set; }
    public string PilotName { get; set; }
    public string Country { get; set; }
    public double? AverageGapPercent { get; set; }
    public double? AverageGapChange { get; set; }
    public int Rank { get; set; }
    public int? RankChange { get; set; }
    public string? League { get; set; }
}

public class LeagueSettingsModel
{
    public bool Enabled { get; set; }
    public string? OthersName { get; set; }
    public List<LeagueDescriptorModel> Descriptors { get; set; } = [];
}

public class LeagueDescriptorModel
{
    public required string Name { get; set; }
    public int Size { get; set; }
    public int Order { get; set; }
    public string? Color { get; set; }
}
