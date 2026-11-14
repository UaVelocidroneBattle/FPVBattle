namespace Veloci.Logic.API.Velocidrone.Dto;

public class ModelsDto : VelocidroneResponse
{
    public ICollection<ModelDto> models { get; set; }
}

public class ModelDto
{
    public int model_id { get; set; }
    public string model_name { get; set; }
    public int quad_class { get; set; }
}
