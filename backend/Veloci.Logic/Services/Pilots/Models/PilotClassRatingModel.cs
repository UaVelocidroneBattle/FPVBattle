namespace Veloci.Logic.Services.Pilots.Models;

/// <summary>
/// A pilot's standing within one configured cup (competition class): their global rating
/// and, if leagues are configured for that cup, their current league.
/// </summary>
public class PilotClassRatingModel
{
    public required string CupId { get; set; }
    public required string ClassName { get; set; }
    public int? GlobalRating { get; set; }
    public string? League { get; set; }
    public string? LeagueColor { get; set; }
    public required List<PilotRatingHistoryPoint> RatingHistory { get; set; }
}
