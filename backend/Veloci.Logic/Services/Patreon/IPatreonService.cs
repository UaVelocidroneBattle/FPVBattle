using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public interface IPatreonService
{
    Task<PatreonCampaign[]> GetCampaignsAsync();
    Task<List<PatreonSupporter>> GetCampaignMembersAsync(string campaignId);
}
