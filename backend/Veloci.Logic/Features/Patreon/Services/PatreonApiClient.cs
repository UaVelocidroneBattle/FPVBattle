using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public class PatreonApiClient : IPatreonApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PatreonApiClient> _logger;
    private readonly IPatreonTokenManager _tokenManager;

    public PatreonApiClient(HttpClient httpClient, IPatreonTokenManager tokenManager, ILogger<PatreonApiClient> logger)
    {
        _httpClient = httpClient;
        _tokenManager = tokenManager;
        _logger = logger;
    }

    public async Task<PatreonCampaign[]> GetCampaignsAsync()
    {
        try
        {
            var response = await MakeAuthenticatedRequestAsync("campaigns");

            if (response == null)
            {
                _logger.LogError("Failed to get campaigns from Patreon API");
                return [];
            }

            var campaignsJson = await response.Content.ReadAsStringAsync();
            var campaignsData = JsonSerializer.Deserialize<PatreonCampaignsResponse>(campaignsJson,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            return campaignsData?.Data.ToArray() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaigns from Patreon API");
            return [];
        }
    }

    public async Task<PatreonMembersResponse> GetCampaignMembersAsync(string campaignId)
    {
        var allMembers = new List<PatreonMember>();
        var allIncluded = new List<PatreonIncluded>();
        var url =
            $"campaigns/{campaignId}/members?include=currently_entitled_tiers,user&fields[member]=full_name,email,patron_status,currently_entitled_amount_cents,pledge_relationship_start&fields[user]=full_name,email&fields[tier]=title";

        while (!string.IsNullOrEmpty(url))
        {
            try
            {
                var response = await MakeAuthenticatedRequestAsync(url);

                if (response == null)
                {
                    _logger.LogError("Failed to get campaign members from Patreon API");
                    break;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<PatreonMembersResponse>(json,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

                if (data?.Data != null)
                {
                    allMembers.AddRange(data.Data);
                }

                if (data?.Included != null)
                {
                    allIncluded.AddRange(data.Included);
                }

                url = data?.Links?.Next;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing campaign members page");
                break;
            }
        }

        return new PatreonMembersResponse { Data = allMembers, Included = allIncluded };
    }

    private async Task<HttpResponseMessage?> MakeAuthenticatedRequestAsync(string url)
    {
        try
        {
            var accessToken = await _tokenManager.GetValidAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No valid Patreon access token available");
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            // If 401 Unauthorized, attempt to refresh token for next job run
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Received 401 Unauthorized, attempting to refresh token");
                
                var tokens = await _tokenManager.GetCurrentTokensAsync();
                if (tokens != null)
                {
                    var newAccessToken = await _tokenManager.RefreshAccessTokenAsync(tokens.RefreshToken);
                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        _logger.LogInformation("Successfully refreshed token for next job run");
                    }
                    else
                    {
                        _logger.LogError("Failed to refresh token");
                    }
                }
            }

            _logger.LogError("Request failed with status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making authenticated request to {Url}", url);
            return null;
        }
    }
}