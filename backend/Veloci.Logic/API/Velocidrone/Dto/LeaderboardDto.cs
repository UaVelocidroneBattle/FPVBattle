namespace Veloci.Logic.API.Velocidrone.Dto;

public class LeaderboardDto : VelocidroneResponse
{
    public ICollection<TrackTimeDto> tracktimes { get; set; }
    public string message_title { get; set; }
}

public class TrackTimeDto
{
    public string lap_time { get; set; }
    public string playername { get; set; }
    public string model_name { get; set; }
    public string country { get; set; }
    public string sim_version { get; set; }
    public int device_type { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public int user_id { get; set; }
}
