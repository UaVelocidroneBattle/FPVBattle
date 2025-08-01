using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API.Options;

namespace Veloci.Logic.Services;

public class PatreonService : IPatreonService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PatreonOptions _options;
    private readonly ILogger<PatreonService> _logger;
    private readonly IRepository<PatreonTokens> _tokensRepository;

    public PatreonService(HttpClient httpClient, IHttpClientFactory httpClientFactory, IOptions<PatreonOptions> options, ILogger<PatreonService> logger, IRepository<PatreonTokens> tokensRepository)
    {
        _httpClient = httpClient;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
        _tokensRepository = tokensRepository;
    }

    public async Task<PatreonTokens?> GetCurrentTokensAsync()
    {
        try
        {
            // First try to get tokens from database (most up-to-date)
            var dbTokens = await _tokensRepository.GetAll().FirstOrDefaultAsync();

            if (dbTokens != null)
            {
                _logger.LogDebug("Using tokens from database");
                return dbTokens;
            }

            // Fallback to configuration tokens (initial setup)
            if (!string.IsNullOrEmpty(_options.AccessToken) && !string.IsNullOrEmpty(_options.RefreshToken))
            {
                _logger.LogDebug("Using tokens from configuration");
                var configTokens = new PatreonTokens();
                configTokens.UpdateFromTokenResponse(_options.AccessToken, _options.RefreshToken, 86400); // Assume 24h expiry
                return configTokens;
            }

            _logger.LogWarning("No Patreon tokens found in database or configuration");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current Patreon tokens");
            return null;
        }
    }

    public async Task<string?> GetValidAccessTokenAsync()
    {
        try
        {
            var tokens = await GetCurrentTokensAsync();
            if (tokens == null)
            {
                _logger.LogWarning("No Patreon tokens available");
                return null;
            }

            // Check if token is expired or expiring soon (10 minute buffer)
            if (tokens.IsExpiringSoon(10))
            {
                _logger.LogInformation("Patreon access token is expiring soon, refreshing...");

                var newAccessToken = await RefreshAccessTokenAsync(tokens.RefreshToken);
                if (newAccessToken != null)
                {
                    return newAccessToken;
                }

                _logger.LogWarning("Failed to refresh Patreon access token, using existing token");
            }

            return tokens.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting valid Patreon access token");
            return null;
        }
    }

    public async Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn, string? scope = null)
    {
        try
        {
            var existingTokens = await _tokensRepository.GetAll().FirstOrDefaultAsync();

            if (existingTokens != null)
            {
                existingTokens.UpdateFromTokenResponse(accessToken, refreshToken, expiresIn, scope);
                await _tokensRepository.UpdateAsync(existingTokens);
            }
            else
            {
                var newTokens = new PatreonTokens();
                newTokens.UpdateFromTokenResponse(accessToken, refreshToken, expiresIn, scope);
                await _tokensRepository.AddAsync(newTokens);
            }

            await _tokensRepository.SaveChangesAsync();
            _logger.LogInformation("Updated Patreon tokens in database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing Patreon tokens to database");
        }
    }

    public async Task<List<PatreonSupporter>> GetCampaignMembersAsync()
    {
        try
        {
            var accessToken = await GetValidAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No valid Patreon access token available");
                return new List<PatreonSupporter>();
            }

            var response = await MakeAuthenticatedRequestAsync("campaigns", accessToken);

            if (response == null)
            {
                _logger.LogError("Failed to get campaigns from Patreon API after retry");
                return new List<PatreonSupporter>();
            }

            var campaignsJson = await response.Content.ReadAsStringAsync();
            var campaignsData = JsonSerializer.Deserialize<PatreonCampaignsResponse>(campaignsJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (campaignsData?.Data == null || !campaignsData.Data.Any())
            {
                _logger.LogWarning("No campaigns found in Patreon response");
                return new List<PatreonSupporter>();
            }

            var campaignId = campaignsData.Data.First().Id;
            return await GetCampaignMembersAsync(campaignId, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign members from Patreon API");
            return new List<PatreonSupporter>();
        }
    }

    private async Task<List<PatreonSupporter>> GetCampaignMembersAsync(string campaignId, string accessToken)
    {
        var supporters = new List<PatreonSupporter>();
        var url = $"campaigns/{campaignId}/members?include=currently_entitled_tiers,user&fields[member]=full_name,email,patron_status,currently_entitled_amount_cents,pledge_relationship_start&fields[user]=full_name,email&fields[tier]=title";

        while (!string.IsNullOrEmpty(url))
        {
            try
            {
                var response = await MakeAuthenticatedRequestAsync(url, accessToken);

                if (response == null)
                {
                    _logger.LogError("Failed to get campaign members from Patreon API after retry");
                    break;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<PatreonMembersResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (data?.Data != null)
                {
                    foreach (var member in data.Data)
                    {
                        var supporter = CreatePatreonSupporter(member, data);
                        if (supporter != null)
                        {
                            supporters.Add(supporter);
                        }
                    }
                }

                url = data?.Links?.Next;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing campaign members page");
                break;
            }
        }

        return supporters;
    }

    private async Task<HttpResponseMessage?> MakeAuthenticatedRequestAsync(string url, string accessToken, int maxRetries = 1)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                // If 401 Unauthorized and we haven't exhausted retries, try to refresh token
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && attempt < maxRetries)
                {
                    _logger.LogWarning("Received 401 Unauthorized, attempting to refresh token (attempt {Attempt}/{MaxRetries})",
                        attempt + 1, maxRetries + 1);

                    var tokens = await GetCurrentTokensAsync();
                    if (tokens != null)
                    {
                        var newAccessToken = await RefreshAccessTokenAsync(tokens.RefreshToken);
                        if (!string.IsNullOrEmpty(newAccessToken))
                        {
                            accessToken = newAccessToken; // Use refreshed token for retry
                            _logger.LogInformation("Successfully refreshed token, retrying request");
                            continue; // Retry with new token
                        }
                    }

                    _logger.LogError("Failed to refresh token for retry");
                }

                _logger.LogError("Request failed with status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making authenticated request to {Url} (attempt {Attempt})", url, attempt + 1);

                if (attempt == maxRetries)
                {
                    return null;
                }
            }
        }

        return null;
    }

    private PatreonSupporter? CreatePatreonSupporter(PatreonMember member, PatreonMembersResponse response)
    {
        try
        {
            var user = response.Included?.FirstOrDefault(i => i.Type == "user" && i.Id == member.Relationships?.User?.Data?.Id);
            var tier = GetMemberTier(member, response);

            return new PatreonSupporter
            {
                PatreonId = member.Id,
                Name = member.Attributes?.FullName ?? user?.Attributes?.FullName ?? "Unknown",
                Email = member.Attributes?.Email ?? user?.Attributes?.Email,
                TierName = tier?.Attributes?.Title,
                Amount = member.Attributes?.CurrentlyEntitledAmountCents.HasValue == true
                    ? member.Attributes.CurrentlyEntitledAmountCents.Value / 100m
                    : null,
                Status = member.Attributes?.PatronStatus ?? "unknown",
                FirstSupportedAt = member.Attributes?.PledgeRelationshipStart ?? DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PatreonSupporter from member data");
            return null;
        }
    }

    private PatreonTier? GetMemberTier(PatreonMember member, PatreonMembersResponse response)
    {
        var tierIds = member.Relationships?.CurrentlyEntitledTiers?.Data?.Select(t => t.Id) ?? Array.Empty<string>();
        return response.Included?.FirstOrDefault(i => i.Type == "tier" && tierIds.Contains(i.Id)) as PatreonTier;
    }

    public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.ClientSecret))
            {
                _logger.LogError("Patreon client credentials not configured");
                return null;
            }

            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            };

            using var oauthClient = _httpClientFactory.CreateClient("PatreonOAuth");
            var response = await oauthClient.PostAsync("token", new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to refresh Patreon access token: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<PatreonTokenResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (tokenData != null)
            {
                // Store the refreshed tokens in database
                await UpdateStoredTokensAsync(tokenData.AccessToken, tokenData.RefreshToken, tokenData.ExpiresIn, tokenData.Scope);
                _logger.LogInformation("Successfully refreshed and stored Patreon access token");
                return tokenData.AccessToken;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Patreon access token");
            return null;
        }
    }
}

// DTOs for Patreon API responses
public class PatreonCampaignsResponse
{
    public List<PatreonCampaign> Data { get; set; } = new();
}

public class PatreonCampaign
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
}

public class PatreonMembersResponse
{
    public List<PatreonMember> Data { get; set; } = new();
    public List<PatreonIncluded> Included { get; set; } = new();
    public PatreonLinks? Links { get; set; }
}

public class PatreonMember
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
    public PatreonMemberAttributes? Attributes { get; set; }
    public PatreonMemberRelationships? Relationships { get; set; }
}

public class PatreonMemberAttributes
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PatronStatus { get; set; }
    public int? CurrentlyEntitledAmountCents { get; set; }
    public DateTime? PledgeRelationshipStart { get; set; }
}

public class PatreonMemberRelationships
{
    public PatreonRelationshipData? User { get; set; }
    public PatreonRelationshipDataList? CurrentlyEntitledTiers { get; set; }
}

public class PatreonRelationshipData
{
    public PatreonRelationshipItem? Data { get; set; }
}

public class PatreonRelationshipDataList
{
    public List<PatreonRelationshipItem>? Data { get; set; }
}

public class PatreonRelationshipItem
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
}

public class PatreonIncluded
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
    public PatreonIncludedAttributes? Attributes { get; set; }
}

public class PatreonIncludedAttributes
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
}

public class PatreonTier : PatreonIncluded
{
}

public class PatreonLinks
{
    public string? Next { get; set; }
}

public class PatreonTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
}
