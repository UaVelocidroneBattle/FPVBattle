using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Application.Features.Teams.Leaderboard;

/// <summary>
/// Finds the team each pilot flies for. Only public teams compete, so smaller teams give no entry.
/// </summary>
public class TeamRosterLookup
{
    private readonly IApplicationDbContext _db;
    private readonly TeamSettings _settings;

    public TeamRosterLookup(IApplicationDbContext db, IOptions<TeamSettings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }

    public async Task<IReadOnlyDictionary<int, int>> GetTeamIdsByPilotIdAsync(CancellationToken ct = default)
    {
        return await _db.TeamMembers
            .Where(m => m.User.PilotId != null && m.Team.Members.Count >= _settings.MinPublicTeamSize)
            .ToDictionaryAsync(m => m.User.PilotId!.Value, m => m.TeamId, ct);
    }
}
