using Veloci.Data.Domain;
using Veloci.Logic.Services.Patreon.Models;

namespace Veloci.Logic.Services;

public interface IPatreonService
{
    Task<PatreonCampaign[]> GetCampaignsAsync();
    Task<PatreonSupporter[]> GetCampaignMembersAsync(string campaignId);
}
