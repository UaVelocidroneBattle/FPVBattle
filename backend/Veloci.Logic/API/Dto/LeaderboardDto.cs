namespace Veloci.Logic.API.Dto;

public class LeaderboardDto
{
    public ICollection<TrackTimeDto> tracktimes { get; set; }
    public string message_title { get; set; }
    public string message { get; set; }
    public bool success { get; set; }
}
