using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Veloci.Logic.Features.Patreon.Exceptions;
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

    public async Task<PatreonCampaign[]> GetCampaignsAsync(CancellationToken ct = default)
    {
        var campaignsData = await MakeAuthenticatedRequestAsync<PatreonCampaignsResponse>("campaigns", ct);
        return campaignsData?.Data.ToArray() ?? [];
    }

    public async Task<PatreonMembersResponse> GetCampaignMembersAsync(string campaignId, CancellationToken ct = default)
    {
        var allMembers = new List<PatreonMember>();
        var allIncluded = new List<PatreonIncluded>();
        var url =
            $"campaigns/{campaignId}/members?include=currently_entitled_tiers,user&fields[member]=full_name,email,patron_status,currently_entitled_amount_cents,pledge_relationship_start&fields[user]=full_name,email&fields[tier]=title";

        while (!string.IsNullOrEmpty(url))
        {
            ct.ThrowIfCancellationRequested();
            
            var data = await MakeAuthenticatedRequestAsync<PatreonMembersResponse>(url, ct);

            if (data == null)
            {
                break;
            }

            if (data.Data != null)
            {
                allMembers.AddRange(data.Data);
            }

            if (data.Included != null)
            {
                allIncluded.AddRange(data.Included);
            }

            url = data.Links?.Next;
        }

        return new PatreonMembersResponse { Data = allMembers, Included = allIncluded };
    }

    /// <summary>
    /// Makes an authenticated GET request to the Patreon API and deserializes the JSON response.
    /// Throws specific exceptions for different failure scenarios.
    /// On 401 responses, attempts to refresh the access token for future requests.
    /// </summary>
    private async Task<T?> MakeAuthenticatedRequestAsync<T>(string url, CancellationToken ct = default) where T : class
    {
        _logger.LogDebug("Making Patreon API request to {Url}", url);

        try
        {
            var accessToken = await _tokenManager.GetValidAccessTokenAsync(ct);
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new PatreonAuthenticationException("No valid Patreon access token available")
                {
                    Endpoint = url
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
                
                if (result == null)
                {
                    throw new PatreonApiException($"Failed to deserialize Patreon API response")
                    {
                        Endpoint = url,
                        StatusCode = response.StatusCode,
                        ResponseContent = string.IsNullOrEmpty(responseContent) ? "[empty]" : responseContent
                    };
                }
                
                return result;
            }

            // Handle specific HTTP status codes
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    _logger.LogWarning("Received 401 Unauthorized for {Url}, attempting to refresh token", url);
                    
                    var tokens = await _tokenManager.GetCurrentTokensAsync(ct);
                    var tokenRefreshAttempted = false;
                    
                    if (tokens != null)
                    {
                        var newAccessToken = await _tokenManager.RefreshAccessTokenAsync(tokens.RefreshToken, ct);
                        tokenRefreshAttempted = true;
                        
                        if (string.IsNullOrEmpty(newAccessToken))
                        {
                            _logger.LogError("Failed to refresh token for {Url}", url);
                        }
                        else
                        {
                            _logger.LogInformation("Successfully refreshed token for next request");
                        }
                    }
                    
                    throw new PatreonAuthenticationException($"Authentication failed for Patreon API request")
                    {
                        Endpoint = url,
                        StatusCode = response.StatusCode,
                        ResponseContent = responseContent,
                        TokenRefreshAttempted = tokenRefreshAttempted
                    };

                case HttpStatusCode.TooManyRequests:
                    var retryAfterHeader = response.Headers.RetryAfter;
                    var retryAfter = retryAfterHeader?.Delta ?? TimeSpan.FromMinutes(1);
                    
                    throw new PatreonRateLimitException($"Rate limit exceeded for Patreon API request")
                    {
                        Endpoint = url,
                        StatusCode = response.StatusCode,
                        ResponseContent = responseContent,
                        RetryAfter = retryAfter
                    };

                default:
                    throw new PatreonApiException($"Patreon API request failed with status: {response.StatusCode}")
                    {
                        Endpoint = url,
                        StatusCode = response.StatusCode,
                        ResponseContent = responseContent
                    };
            }
        }
        catch (PatreonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PatreonApiException($"Unexpected error during Patreon API request", ex)
            {
                Endpoint = url
            };
        }
    }
}