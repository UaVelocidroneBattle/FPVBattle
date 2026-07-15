namespace Veloci.Web.Controllers.Landing;

public class LandingDataModel
{
    public int TotalPilots { get; set; }
    public int TotalCountries { get; set; }
    public int DailyActivePilots { get; set; }
    public IEnumerable<CountryPilotsModel> CountryPilots { get; set; } = new List<CountryPilotsModel>();
}

public class CountryPilotsModel
{
    public string Country { get; set; }
    public string CountryCode { get; set; }
    public int Pilots { get; set; }
}
