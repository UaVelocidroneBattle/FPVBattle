namespace Veloci.Logic.Services.Statistics.YearResults;

public class YearResultsModel
{
    public int Year { get; set; }
    public int TotalTrackCount { get; set; }
    public int UniqueTrackCount { get; set; }
    public int TracksSkipped { get; set; }
    public string FavoriteTrack { get; set; }
    public int TotalPilotCount { get; set; }
    public (string name, int count) PilotWhoCameTheMost { get; set; }
    public (string name, int count) PilotWhoCameTheLeast { get; set; }
    public (string name, int count) PilotWithTheMostGoldenMedal { get; set; }
    public Dictionary<string, int> Top3Pilots { get; set; }
}
