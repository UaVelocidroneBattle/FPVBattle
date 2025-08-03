using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

/// <summary>
/// Client interface for interacting with Patreon API v2.
/// Provides methods to retrieve campaign and member data using OAuth authentication.
/// </summary>
public interface IPatreonApiClient
{
    /// <summary>
    /// Retrieves all campaigns associated with the authenticated user.
    /// A campaign represents a creator's Patreon page with details like name, summary, and patron count.
    /// </summary>
    Task<PatreonCampaign[]> GetCampaignsAsync();
    
    /// <summary>
    /// Retrieves all members (patrons) for a specific campaign.
    /// Members represent users who support a campaign with pledges and subscriptions.
    /// </summary>
    /// <param name="campaignId">The unique identifier of the campaign</param>
    Task<PatreonMembersResponse> GetCampaignMembersAsync(string campaignId);
}