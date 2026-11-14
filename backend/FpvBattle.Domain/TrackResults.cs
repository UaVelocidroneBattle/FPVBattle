namespace FpvBattle.Domain;

public class TrackResults
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public virtual List<TrackTime> Times { get; set; } = new();
}