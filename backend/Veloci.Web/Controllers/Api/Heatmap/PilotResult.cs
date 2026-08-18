namespace Veloci.Web.Controllers.Api.Heatmap;

public class PilotResult
{
    /// <summary>
    /// Date, when points were earned.
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// Earned points.
    /// </summary>
    public required int Points { get; set; }

    /// <summary>
    /// Track time in milliseconds
    /// </summary>
    public required int TrackTime { get; set; }
}
