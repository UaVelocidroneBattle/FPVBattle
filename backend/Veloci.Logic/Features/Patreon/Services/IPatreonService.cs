using Veloci.Data.Domain;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

/// <summary>
///     High-level service for retrieving and processing Patreon campaign and supporter data.
/// </summary>
public interface IPatreonService
{
    /// <summary>
    ///     Retrieves all campaigns for the authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Array of campaigns with basic information</returns>
    Task<PatreonCampaign[]> GetCampaignsAsync(CancellationToken ct);

    /// <summary>
    ///     Retrieves and processes all supporters for a campaign, including user and tier information.
    /// </summary>
    /// <param name="campaignId">Campaign identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Array of supporters with complete profile data</returns>
    Task<PatreonSupporter[]> GetCampaignMembersAsync(string campaignId, CancellationToken ct);
}
