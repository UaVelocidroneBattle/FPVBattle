using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
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
        var payload = $"track_id={trackId}&sim_version=1.16&offset=0&count=10000&race_mode=6";
        var postData = $"post_data={Uri.EscapeDataString(payload)}";

        var response = await DoRequestAsync<LeaderboardDto>("api/leaderboard", HttpMethod.Post, postData);
        return response.tracktimes;
    }

    private async Task<T> DoRequestAsync<T>(string uri, HttpMethod method, string? formData = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

        if (formData is not null)
        {
            request.Content = new StringContent(formData, Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<T>(content);

            if (data is not null)
                return data;

            throw new Exception("Response deserialized as null.");
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Request failed: {response.StatusCode}, {error}");
    }
}
