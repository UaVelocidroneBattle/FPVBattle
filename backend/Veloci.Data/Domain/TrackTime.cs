using System.ComponentModel.DataAnnotations;

namespace Veloci.Data.Domain;

public class TrackTime
{
    public TrackTime()
    {
    }

    public TrackTime(int globalRank, string name, int time)
    {
        GlobalRank = globalRank;
        PlayerName = name;
        Time = time;
    }

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int Time { get; set; }

    [MaxLength(128)]
    public string PlayerName { get; set; }

    [MaxLength(128)]
    public string ModelName { get; set; }

    public int GlobalRank { get; set; }

    public int LocalRank { get; set; }
}
