using FpvBattle.Domain;
using FpvBattle.Logic.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Logic.Features.Teams;

/// <summary>
/// Finds the tag a pilot wears on leaderboards: that of their team. Teams that are not public yet give no tag.
/// </summary>
public class TeamTagLookup
{
    private readonly IApplicationDbContext _db;
    private readonly TeamSettings _settings;

    public TeamTagLookup(IApplicationDbContext db, IOptions<TeamSettings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }

    /// <summary>
    /// Pilot names are case-sensitive, hence the ordinal dictionary.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, string>> GetTagsByPilotNameAsync(CancellationToken ct = default)
    {
        return await _db.TeamMembers
            .Where(m => m.Team.Tag != null && m.User.Pilot != null && m.Team.Members.Count >= _settings.MinPublicTeamSize)
            .Select(m => new { PilotName = m.User.Pilot!.Name, Tag = m.Team.Tag! })
            .ToDictionaryAsync(m => m.PilotName, m => m.Tag, StringComparer.Ordinal, ct);
    }
}
