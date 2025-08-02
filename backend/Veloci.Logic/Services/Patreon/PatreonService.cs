using Microsoft.Extensions.Logging;
using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public class PatreonService : IPatreonService
{
    private readonly IPatreonApiClient _apiClient;
    private readonly ILogger<PatreonService> _logger;

    public PatreonService(IPatreonApiClient apiClient, ILogger<PatreonService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<PatreonCampaign>> GetCampaignsAsync()
    {
        try
        {
            return await _apiClient.GetCampaignsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaigns");
            return new List<PatreonCampaign>();
        }
    }

    public async Task<List<PatreonSupporter>> GetCampaignMembersAsync(string campaignId)
    {
        try
        {
            var membersResponse = await _apiClient.GetCampaignMembersAsync(campaignId);

            var supporters = new List<PatreonSupporter>();
            foreach (var member in membersResponse.Data)
            {
                var supporter = CreatePatreonSupporter(member, membersResponse);
                if (supporter != null)
                {
                    supporters.Add(supporter);
                }
            }

            return supporters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign members for campaign {CampaignId}", campaignId);
            return new List<PatreonSupporter>();
        }
    }

    private PatreonSupporter? CreatePatreonSupporter(PatreonMember member, PatreonMembersResponse response)
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
            _logger.LogError(ex, "Error creating PatreonSupporter from member data");
            return null;
        }
    }

    private PatreonTier? GetMemberTier(PatreonMember member, PatreonMembersResponse response)
    {
        var tierIds = member.Relationships?.CurrentlyEntitledTiers?.Data?.Select(t => t.Id) ?? [];
        return response.Included?.FirstOrDefault(i => i.Type == "tier" && tierIds.Contains(i.Id)) as PatreonTier;
    }
}
