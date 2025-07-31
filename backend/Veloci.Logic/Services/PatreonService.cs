using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public class PatreonService : IPatreonService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PatreonService> _logger;

    public PatreonService(HttpClient httpClient, IConfiguration configuration, ILogger<PatreonService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<PatreonSupporter>> GetCampaignMembersAsync()
    {
        try
        {
            var accessToken = _configuration["Patreon:AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Patreon access token not configured");
                return new List<PatreonSupporter>();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.patreon.com/api/oauth2/v2/campaigns");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get campaigns from Patreon API: {StatusCode}", response.StatusCode);
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
        var url = $"https://www.patreon.com/api/oauth2/v2/campaigns/{campaignId}/members?include=currently_entitled_tiers,user&fields[member]=full_name,email,patron_status,currently_entitled_amount_cents,pledge_relationship_start&fields[user]=full_name,email&fields[tier]=title";

        while (!string.IsNullOrEmpty(url))
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get campaign members from Patreon API: {StatusCode}", response.StatusCode);
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
            var clientId = _configuration["Patreon:ClientId"];
            var clientSecret = _configuration["Patreon:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Patreon client credentials not configured");
                return null;
            }

            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.patreon.com/api/oauth2/token")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await _httpClient.SendAsync(request);
            
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

            return tokenData?.AccessToken;
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
}