using Microsoft.Extensions.Logging;
using Veloci.Data.Domain;
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
    ///     Uses PatreonMemberProcessor for efficient data transformation with optimized lookups.
    /// </summary>
    public async Task<PatreonSupporter[]> GetCampaignMembersAsync(string campaignId, CancellationToken ct)
    {
        _logger.LogDebug("Fetching campaign members for campaign {CampaignId}", campaignId);

        var membersResponse = await _apiClient.GetCampaignMembersAsync(campaignId, ct);

        var processor = new PatreonMemberProcessor(membersResponse);
        var supporters = processor.ProcessMembers(membersResponse);

        _logger.LogDebug("Successfully processed {SupporterCount} supporters for campaign {CampaignId}",
            supporters.Length, campaignId);

        return supporters;
    }
}
