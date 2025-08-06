using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Logic.API.Dto;
using Veloci.Logic.API.Exceptions;
using Veloci.Logic.API.Options;
using Veloci.Logic.Services;

namespace Veloci.Logic.API;

public class Velocidrone
{
    private static readonly ILogger _log = Log.ForContext<Velocidrone>();
    
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
        _log.Debug("Requesting leaderboard for track {TrackId} from Velocidrone API", trackId);
        
        var payload = $"track_id={trackId}&sim_version=1.16&offset=0&count=1000&race_mode=6";
        var postData = $"post_data={Uri.EscapeDataString(payload)}";

        var response = await DoRequestAsync<LeaderboardDto>("api/leaderboard", HttpMethod.Post, postData);
        
        _log.Information("Retrieved {ResultCount} results from Velocidrone API for track {TrackId}", 
            response.tracktimes.Count, trackId);
            
        return response.tracktimes;
    }

    private async Task<T> DoRequestAsync<T>(string uri, HttpMethod method, string? formData = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _log.Debug("Making {Method} request to Velocidrone API endpoint: {Uri}", method, uri);
        
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

        if (formData is not null)
        {
            request.Content = new StringContent(formData, Encoding.UTF8, "application/x-www-form-urlencoded");
            _log.Debug("Request includes form data: {FormDataLength} characters", formData.Length);
        }

        try
        {
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            _log.Debug("Velocidrone API responded with status {StatusCode} in {Duration}ms", 
                response.StatusCode, stopwatch.ElapsedMilliseconds);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _log.Debug("Received {ContentLength} characters in response body", content.Length);
                
                // Check if response is HTML (login page) indicating authentication failure
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _log.Error("Velocidrone API returned HTML login page for endpoint {Uri}, indicating invalid credentials", uri);
                    throw new VelocidroneAuthenticationException(uri, response.StatusCode, content);
                }
                
                var data = JsonSerializer.Deserialize<T>(content);

                if (data is not null)
                {
                    _log.Debug("Successfully deserialized Velocidrone API response to {ResponseType}", typeof(T).Name);
                    return data;
                }

                _log.Error("Velocidrone API response deserialized as null for endpoint {Uri}", uri);
                throw new Exception("Response deserialized as null.");
            }

            var error = await response.Content.ReadAsStringAsync();
            _log.Error("Velocidrone API request failed for {Uri}: {StatusCode} - {Error}", 
                uri, response.StatusCode, error);
            throw new Exception($"Request failed: {response.StatusCode}, {error}");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _log.Error(ex, "Network error during Velocidrone API request to {Uri} after {Duration}ms", 
                uri, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            _log.Error(ex, "Velocidrone API request to {Uri} timed out after {Duration}ms", 
                uri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
