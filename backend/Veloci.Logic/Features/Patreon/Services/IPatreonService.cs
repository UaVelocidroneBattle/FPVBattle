using Veloci.Data.Domain;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public interface IPatreonService
{
    Task<PatreonCampaign[]> GetCampaignsAsync();
    Task<PatreonSupporter[]> GetCampaignMembersAsync(string campaignId);
}