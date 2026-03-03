using System.Text.Json;
using System.Text.Json.Serialization;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Logic.Services.Tracks;

public class ApiTrackFetcher : ITrackFetcher
{
    private static readonly HttpClient Client = new HttpClient();

    public async Task<List<ParsedMapModel>> FetchMapsAsync()
    {
        var response = await Client.GetAsync($"{VelocidroneApiConstants.BaseUrl}/api/get_official_tracks");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var decrypted = Encryption.Decrypt(responseBody);
        var results = JsonSerializer.Deserialize<TrackApiResponse>(decrypted);

        var scenes = results.Tracks
            .GroupBy(t => t.SceneId)
            .Select(group =>
            {
                var map = new ParsedMapModel
                {
                    Id = group.Key,
                    Name = group.Key.ToString(), // setting scene id to name as well since we do not get scene name from API
                    Tracks = group.Select(t => new ParsedTrackModel
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Url = t.Url,
                        Type = t.Type,
                    }).ToList()
                };

                foreach (var track in map.Tracks)
                    track.Map = map;

                return map;
            })
            .ToList();

        return scenes;
    }
}

public class TrackApiResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("success")]
    public bool success { get; set; }

    [JsonPropertyName("tracks")]
    public List<TrackApi> Tracks { get; set; }
}

public class TrackApi
{
    [JsonPropertyName("track_url")]
    public string Url { get; set; }

    [JsonPropertyName("track_name")]
    public string Name { get; set; }

    [JsonPropertyName("track_id")]
    public int Id { get; set; }

    [JsonPropertyName("scene_id")]
    public int SceneId { get; set; }

    [JsonPropertyName("ver")]
    public int Version { get; set; }

    //TODO: convert to date time later
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("count")]
    public double Count { get; set; }
}
