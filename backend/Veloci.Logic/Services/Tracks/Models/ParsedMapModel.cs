namespace Veloci.Logic.Services.Tracks.Models;

public class ParsedMapModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public List<ParsedTrackModel> Tracks { get; set; } = new();
}

public class ParsedTrackModel
{
    public ParsedMapModel Map { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public int Type { get; set; }
}
