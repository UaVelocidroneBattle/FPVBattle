using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Teams.Leaderboard;

/// <summary>
/// Finds the team each pilot flies for. Only public teams compete, so smaller teams give no entry.
/// </summary>
public class TeamRosterLookup
{
    private readonly IRepository<TeamMember> _teamMembers;
    private readonly TeamSettings _settings;

    public TeamRosterLookup(IRepository<TeamMember> teamMembers, IOptions<TeamSettings> settings)
    {
        _teamMembers = teamMembers;
        _settings = settings.Value;
    }

    public async Task<IReadOnlyDictionary<int, int>> GetTeamIdsByPilotIdAsync(CancellationToken ct = default)
    {
        return await _teamMembers.GetAll()
            .Where(m => m.User.PilotId != null && m.Team.Members.Count >= _settings.MinPublicTeamSize)
            .ToDictionaryAsync(m => m.User.PilotId!.Value, m => m.TeamId, ct);
    }
}
