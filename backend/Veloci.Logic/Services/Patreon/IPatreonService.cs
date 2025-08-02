using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public interface IPatreonService
{
    Task<PatreonCampaign[]> GetCampaignsAsync();
    Task<PatreonSupporter[]> GetCampaignMembersAsync(string campaignId);
}
