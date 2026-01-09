using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Veloci.Logic.Features.Patreon.Exceptions;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public class PatreonApiClient : IPatreonApiClient
{
    /// <summary>
    ///     Static JsonSerializerOptions for consistent and efficient JSON parsing across all requests.
    ///     Configured for Patreon API's snake_case naming convention.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

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
        if (string.IsNullOrWhiteSpace(campaignId))
        {
            throw new ArgumentException("Campaign ID cannot be null or empty", nameof(campaignId));
        }

        // Build the initial URL using PatreonUrlBuilder
        var initialUrl = PatreonUrlBuilder.CreateStandardMemberRequest(campaignId).Build();

        return await FetchPaginatedMembersAsync(initialUrl, ct);
    }

    /// <summary>
    ///     Fetches all pages of member data and aggregates them into a single response.
    ///     Handles pagination automatically by following 'next' links.
    /// </summary>
    /// <param name="initialUrl">The initial URL to start fetching from</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Aggregated response containing all members and included data</returns>
    private async Task<PatreonMembersResponse> FetchPaginatedMembersAsync(string initialUrl, CancellationToken ct)
    {
        var allMembers = new List<PatreonMember>();
        var allIncluded = new List<PatreonIncluded>();
        var currentUrl = initialUrl;

        while (!string.IsNullOrEmpty(currentUrl))
        {
            ct.ThrowIfCancellationRequested();

            var data = await MakeAuthenticatedRequestAsync<PatreonMembersResponse>(currentUrl, ct);

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

            currentUrl = data.Links?.Next;
        }

        return new PatreonMembersResponse { Data = allMembers, Included = allIncluded };
    }

    /// <summary>
    ///     Makes an authenticated GET request to the Patreon API and deserializes the JSON response.
    ///     Throws specific exceptions for different failure scenarios.
    ///     On 401 responses, attempts to refresh the access token for future requests.
    /// </summary>
    private async Task<T?> MakeAuthenticatedRequestAsync<T>(string url, CancellationToken ct = default) where T : class
    {
        _logger.LogDebug("Making Patreon API request to {Url}", url);

        try
        {
            var accessToken = await _tokenManager.GetValidAccessTokenAsync(ct);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                return DeserializeResponse<T>(responseContent, url, response.StatusCode);
            }

            // Handle specific HTTP status codes
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    _logger.LogWarning("Received 401 Unauthorized for {Url}, attempting to refresh token", url);

                    var tokenRefreshAttempted = false;

                    try
                    {
                        var tokens = await _tokenManager.GetCurrentTokensAsync(ct);
                        await _tokenManager.RefreshAccessTokenAsync(tokens.RefreshToken, ct);
                        tokenRefreshAttempted = true;
                        _logger.LogInformation("Successfully refreshed token for next request");
                    }
                    catch (PatreonTokenUnavailableException ex)
                    {
                        _logger.LogError(ex, "No tokens available for refresh for {Url}", url);
                    }
                    catch (PatreonTokenRefreshException ex)
                    {
                        tokenRefreshAttempted = true;
                        _logger.LogError(ex, "Failed to refresh token for {Url}", url);
                    }

                    throw new PatreonAuthenticationException("Authentication failed for Patreon API request")
                    {
                        Endpoint = url,
                        StatusCode = response.StatusCode,
                        ResponseContent = responseContent,
                        TokenRefreshAttempted = tokenRefreshAttempted
                    };

                case HttpStatusCode.TooManyRequests:
                    var retryAfterHeader = response.Headers.RetryAfter;
                    var retryAfter = retryAfterHeader?.Delta ?? TimeSpan.FromMinutes(1);

                    throw new PatreonRateLimitException("Rate limit exceeded for Patreon API request")
                    {
                        Endpoint = url,
                        StatusCode = response.StatusCode,
                        ResponseContent = responseContent,
                        RetryAfter = retryAfter
                    };

                default:
                    throw new PatreonApiException($"Patreon API request failed with status: {response.StatusCode}")
                    {
                        Endpoint = url, StatusCode = response.StatusCode, ResponseContent = responseContent
                    };
            }
        }
        catch (PatreonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PatreonApiException("Unexpected error during Patreon API request", ex) { Endpoint = url };
        }
    }

    /// <summary>
    ///     Deserializes JSON response content with enhanced error handling and context preservation.
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="responseContent">Raw JSON response content</param>
    /// <param name="url">API endpoint URL for error context</param>
    /// <param name="statusCode">HTTP status code for error context</param>
    /// <returns>Deserialized object</returns>
    /// <exception cref="PatreonApiException">Thrown when deserialization fails</exception>
    private static T DeserializeResponse<T>(string responseContent, string url, HttpStatusCode statusCode)
        where T : class
    {
        try
        {
            // Handle empty responses
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new PatreonApiException("Received empty response from Patreon API")
                {
                    Endpoint = url, StatusCode = statusCode, ResponseContent = "[empty]"
                };
            }

            var result = JsonSerializer.Deserialize<T>(responseContent, JsonOptions);

            if (result == null)
            {
                throw new PatreonApiException("JSON deserialization returned null result")
                {
                    Endpoint = url,
                    StatusCode = statusCode,
                    ResponseContent = responseContent.Length > 1000
                        ? responseContent.Substring(0, 1000) + "... [truncated]"
                        : responseContent
                };
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new PatreonApiException("Failed to parse JSON response from Patreon API", ex)
            {
                Endpoint = url,
                StatusCode = statusCode,
                ResponseContent = responseContent.Length > 500
                    ? responseContent.Substring(0, 500) + "... [truncated]"
                    : responseContent
            };
        }
    }
}
