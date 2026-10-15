using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Teams;

/// <summary>
/// Finds the tag a pilot wears on leaderboards: that of their team. Teams that are not public yet give no tag.
/// </summary>
public class TeamTagLookup
{
    private readonly IRepository<TeamMember> _teamMembers;
    private readonly TeamSettings _settings;

    public TeamTagLookup(IRepository<TeamMember> teamMembers, IOptions<TeamSettings> settings)
    {
        _teamMembers = teamMembers;
        _settings = settings.Value;
    }

    /// <summary>
    /// Pilot names are case sensitive, hence the ordinal dictionary.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, string>> GetTagsByPilotNameAsync(CancellationToken ct = default)
    {
        return await _teamMembers.GetAll()
            .Where(m => m.Team.Tag != null && m.User.Pilot != null && m.Team.Members.Count >= _settings.MinPublicTeamSize)
            .Select(m => new { PilotName = m.User.Pilot!.Name, Tag = m.Team.Tag! })
            .ToDictionaryAsync(m => m.PilotName, m => m.Tag, StringComparer.Ordinal, ct);
    }
}
