using Microsoft.Extensions.Logging;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Patreon.Exceptions;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public class PatreonService : IPatreonService
{
    private readonly IPatreonApiClient _apiClient;
    private readonly ILogger<PatreonService> _logger;

    public PatreonService(IPatreonApiClient apiClient, ILogger<PatreonService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieves all campaigns for the authenticated user from Patreon API.
    /// </summary>
    public async Task<PatreonCampaign[]> GetCampaignsAsync(CancellationToken ct)
    {
        _logger.LogDebug("Fetching Patreon campaigns");
        return await _apiClient.GetCampaignsAsync(ct);
    }

    /// <summary>
    ///     Retrieves campaign members from Patreon API and processes them into supporter objects.
    ///     Combines member data with user profiles and tier information from API includes.
    /// </summary>
    public async Task<PatreonSupporter[]> GetCampaignMembersAsync(string campaignId, CancellationToken ct)
    {
        _logger.LogDebug("Fetching campaign members for campaign {CampaignId}", campaignId);

        var membersResponse = await _apiClient.GetCampaignMembersAsync(campaignId, ct);

        var supporters = membersResponse.Data.Select(member => CreatePatreonSupporter(member, membersResponse))
            .ToArray();

        _logger.LogDebug("Successfully processed {SupporterCount} supporters for campaign {CampaignId}",
            supporters.Length, campaignId);

        return supporters;
    }

    /// <summary>
    ///     Creates a PatreonSupporter from raw API member data by combining member, user, and tier information.
    /// </summary>
    private PatreonSupporter CreatePatreonSupporter(PatreonMember member, PatreonMembersResponse response)
    {
        try
        {
            var user = response.Included?.FirstOrDefault(i =>
                i.Type == "user" && i.Id == member.Relationships?.User?.Data?.Id);
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
            throw new PatreonMemberParsingException($"Failed to parse member data for member ID: {member.Id}")
            {
                MemberId = member.Id
            };
        }
    }

    /// <summary>
    ///     Extracts the member's current tier from the API response by matching tier IDs.
    /// </summary>
    private static PatreonIncluded? GetMemberTier(PatreonMember member, PatreonMembersResponse response)
    {
        var tierIds = member.Relationships?.CurrentlyEntitledTiers?.Data?.Select(t => t.Id) ?? [];
        return response.Included?.FirstOrDefault(i => i.Type == "tier" && tierIds.Contains(i.Id));
    }
}
