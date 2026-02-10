using Microsoft.EntityFrameworkCore;

namespace Veloci.Data.Domain;

public class TrackTime
{
    public TrackTime()
    {
    }

    public TrackTime(int globalRank, string name, int userId, int time)
    {
        GlobalRank = globalRank;
        PlayerName = name;
        Time = time;
        UserId = userId;
    }

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int Time { get; set; }

    public string PlayerName { get; set; }

    public int? UserId { get; set; }

    public string ModelName { get; set; }

    public int GlobalRank { get; set; }

    public int LocalRank { get; set; }

    public string? TrackResultsId { get; set; }

    public DateTime UpdatedAt { get; set; }
}
