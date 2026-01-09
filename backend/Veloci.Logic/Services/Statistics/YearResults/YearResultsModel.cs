namespace Veloci.Logic.Services.Statistics.YearResults;

public class YearResultsModel
{
    public required int Year { get; set; }
    public required int TotalTrackCount { get; set; }
    public required int UniqueTrackCount { get; set; }
    public required int TracksSkipped { get; set; }
    public required string FavoriteTrack { get; set; }
    public required int TotalPilotCount { get; set; }
    public required (string name, int count) PilotWhoCameTheMost { get; set; }
    public required (string name, int count) PilotWhoCameTheLeast { get; set; }
    public required (string name, int count) PilotWithTheMostGoldenMedal { get; set; }
    public required Dictionary<string, int> Top3Pilots { get; set; }
}
