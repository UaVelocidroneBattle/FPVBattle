using Microsoft.Extensions.Configuration;
using RestSharp;
using Veloci.Logic.API.Dto;
using Veloci.Logic.Services;

namespace Veloci.Logic.API;

public class Velocidrone
{
    private readonly string? _apiToken;

    public Velocidrone(IConfiguration configuration)
    {
        _apiToken = configuration.GetSection("API:Token").Value;
    }

    public async Task<ICollection<TrackTimeDto>> LeaderboardAsync(int trackId)
    {
        var payload = $"track_id={trackId}&sim_version=1.16&offset=0&count=10000&race_mode=6";
        var postData = $"post_data={Uri.EscapeDataString(payload)}";

        var response = await DoRequestAsync<LeaderboardDto>("api/leaderboard", Method.Post, postData);
        return response.tracktimes;
    }

    private async Task<T> DoRequestAsync<T>(string uri, Method method, string? formData = null)
    {
        var client = new RestClient(VelocidroneApiConstants.BaseUrl);
        var request = new RestRequest(uri, method);
        request.AddHeader("Authorization", $"Bearer {_apiToken}");

        if (formData != null)
        {
            request.AddParameter("application/x-www-form-urlencoded", formData, ParameterType.RequestBody);
        }

        var response = await client.ExecuteAsync<T>(request);

        if (response.IsSuccessful && response.Data != null)
        {
            return response.Data;
        }

        throw new Exception(response.ErrorMessage);
    }
}
