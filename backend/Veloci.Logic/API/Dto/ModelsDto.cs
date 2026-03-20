namespace Veloci.Logic.API.Dto;

public class ModelsDto
{
    public ICollection<ModelDto> models { get; set; }
    public bool success { get; set; }
    public string message { get; set; }
}

public class ModelDto
{
    public int model_id { get; set; }
    public string model_name { get; set; }
    public int quad_class { get; set; }
}