namespace Veloci.Logic.API.Velocidrone.Dto;

public class TracksDto : VelocidroneResponse
{
    public ICollection<TrackDto> tracks { get; set; }
}

public class TrackDto
{
    public int track_id { get; set; }
    public string track_name { get; set; }
    public int scenery_id { get; set; }
    public int type { get; set; }
}
