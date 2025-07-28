using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Logic.API.Dto;
using Veloci.Logic.API.Options;
using Veloci.Logic.Services;

namespace Veloci.Logic.API;

public class Velocidrone
{
    private static HttpClient? _httpClient;
    private readonly string? _apiToken;

    public Velocidrone(IOptions<ApiSettings> options)
    {
        _httpClient ??= new HttpClient
        {
            BaseAddress = new Uri(VelocidroneApiConstants.BaseUrl)
        };

        _apiToken = options?.Value?.Token;
    }

    public async Task<ICollection<TrackTimeDto>> LeaderboardAsync(int trackId)
    {
        Log.Debug("Requesting leaderboard for track {TrackId} from Velocidrone API", trackId);
        
        var payload = $"track_id={trackId}&sim_version=1.16&offset=0&count=1000&race_mode=6";
        var postData = $"post_data={Uri.EscapeDataString(payload)}";

        var response = await DoRequestAsync<LeaderboardDto>("api/leaderboard", HttpMethod.Post, postData);
        
        Log.Information("Retrieved {ResultCount} results from Velocidrone API for track {TrackId}", 
            response.tracktimes.Count, trackId);
            
        return response.tracktimes;
    }

    private async Task<T> DoRequestAsync<T>(string uri, HttpMethod method, string? formData = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Log.Debug("Making {Method} request to Velocidrone API endpoint: {Uri}", method, uri);
        
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

        if (formData is not null)
        {
            request.Content = new StringContent(formData, Encoding.UTF8, "application/x-www-form-urlencoded");
            Log.Debug("Request includes form data: {FormDataLength} characters", formData.Length);
        }

        try
        {
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            Log.Debug("Velocidrone API responded with status {StatusCode} in {Duration}ms", 
                response.StatusCode, stopwatch.ElapsedMilliseconds);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Debug("Received {ContentLength} characters in response body", content.Length);
                
                var data = JsonSerializer.Deserialize<T>(content);

                if (data is not null)
                {
                    Log.Debug("Successfully deserialized Velocidrone API response to {ResponseType}", typeof(T).Name);
                    return data;
                }

                Log.Error("Velocidrone API response deserialized as null for endpoint {Uri}", uri);
                throw new Exception("Response deserialized as null.");
            }

            var error = await response.Content.ReadAsStringAsync();
            Log.Error("Velocidrone API request failed for {Uri}: {StatusCode} - {Error}", 
                uri, response.StatusCode, error);
            throw new Exception($"Request failed: {response.StatusCode}, {error}");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Network error during Velocidrone API request to {Uri} after {Duration}ms", 
                uri, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Velocidrone API request to {Uri} timed out after {Duration}ms", 
                uri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
