using System.Net;
using System.Text.Json;
using Serilog;
using Veloci.Logic.API.Velocidrone.Dto;
using Veloci.Logic.API.Velocidrone.Exceptions;

namespace Veloci.Logic.API.Velocidrone;

public class VelocidroneClient
{
    public static readonly Uri BaseAddress = new("https://velocidrone.co.uk");

    private const string SimVersion = "1.16";
    private const int SingleLapRaceMode = 6;
    private const int MaxLeaderboardEntries = 2000;

    private static readonly ILogger Log = Serilog.Log.ForContext<VelocidroneClient>();

    private readonly HttpClient _httpClient;

    public VelocidroneClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ICollection<TrackTimeDto>> LeaderboardAsync(int trackId, CancellationToken cancellationToken = default)
    {
        var payload = $"track_id={trackId}&sim_version={SimVersion}&offset=0&count={MaxLeaderboardEntries}&race_mode={SingleLapRaceMode}";
        var form = new FormUrlEncodedContent([new KeyValuePair<string, string>("post_data", payload)]);

        var response = await PostAsync<LeaderboardDto>("api/leaderboard", form, cancellationToken);
        Log.Information("Retrieved {ResultCount} results from Velocidrone API for track {TrackId}", response.tracktimes.Count, trackId);

        return response.tracktimes;
    }

    public async Task<ICollection<ModelDto>> ModelsAsync(CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<ModelsDto>("api/get-models", content: null, cancellationToken);
        Log.Information("Retrieved {ModelCount} models from Velocidrone API", response.models.Count);

        return response.models;
    }

    public async Task<ICollection<TrackDto>> TracksAsync(CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<TracksDto>("api/get-official-tracks", content: null, cancellationToken);
        Log.Information("Retrieved {TrackCount} tracks from Velocidrone API", response.tracks.Count);

        return response.tracks;
    }

    private async Task<T> PostAsync<T>(string endpoint, HttpContent? content, CancellationToken cancellationToken)
        where T : VelocidroneResponse
    {
        using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        Log.Debug("Velocidrone API {Endpoint} responded with {StatusCode}, {ContentLength} characters",
            endpoint, response.StatusCode, body.Length);

        // An expired or invalid token can also come back as a 200 with the HTML login page
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden || IsHtml(response))
            throw new VelocidroneAuthenticationException(endpoint, response.StatusCode, body);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Velocidrone API {endpoint} failed with {(int)response.StatusCode}: {body}", null, response.StatusCode);

        var data = JsonSerializer.Deserialize<T>(body)
                   ?? throw new VelocidroneApiException(endpoint, "response body is empty");

        if (!data.success)
            throw new VelocidroneApiException(endpoint, data.message);

        return data;
    }

    private static bool IsHtml(HttpResponseMessage response) =>
        response.Content.Headers.ContentType?.MediaType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true;
}
