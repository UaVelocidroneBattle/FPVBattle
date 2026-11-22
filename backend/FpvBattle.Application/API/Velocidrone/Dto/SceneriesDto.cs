namespace FpvBattle.Application.API.Velocidrone.Dto;

public class SceneriesDto : VelocidroneResponse
{
    public ICollection<SceneryDto> sceneries { get; set; }
}

public class SceneryDto
{
    public int scenery_id { get; set; }
    public string scenery_name { get; set; }
}
