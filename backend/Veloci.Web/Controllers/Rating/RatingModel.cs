namespace Veloci.Web.Controllers.Rating;

public class RatingModel
{
    public DateTime CalculatedOn { get; set; }
    public IList<PilotRatingModel> Ratings { get; set; }
}

public class PilotRatingModel
{
    public int PilotId { get; set; }
    public string PilotName { get; set; }
    public double? AverageGapPercent { get; set; }
    public double? AverageGapChange { get; set; }
    public int Rank { get; set; }
    public int? RankChange { get; set; }
}
