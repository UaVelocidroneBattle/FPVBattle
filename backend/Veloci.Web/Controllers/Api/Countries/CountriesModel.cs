namespace Veloci.Web.Controllers.Api.Countries;

public class CountryModel
{
    public string CountryCode { get; set; }
    public string CountryName { get; set; }
    public int PilotsCount { get; set; }
}

public class CountryPilotModel
{
    public int PilotId { get; set; }
    public string PilotName { get; set; }
}
