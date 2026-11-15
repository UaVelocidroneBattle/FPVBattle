using FpvBattle.Application.Features.Patreon;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Application.Features.Teams;

/// <summary>
/// Whether a user may create a team themselves. Site admins creating a team from the admin area skip it.
/// </summary>
public class TeamCreationPolicy
{
    private readonly IApplicationDbContext _db;
    private readonly TeamSettings _settings;

    public TeamCreationPolicy(
        IApplicationDbContext db,
        IOptions<TeamSettings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }

    public async Task<TeamCreationBlocker?> FindBlockerAsync(ApplicationUser user)
    {
        if (user.PilotId is not { } pilotId)
            return TeamCreationBlocker.PilotNotLinked;

        if (await _db.TeamMembers.AnyAsync(m => m.UserId == user.Id))
            return TeamCreationBlocker.AlreadyInTeam;

        if (await HasEnoughRaceDaysAsync(pilotId) || await IsAllowedPatronAsync(pilotId))
            return null;

        return TeamCreationBlocker.NotEnoughRaceDays;
    }

    private async Task<bool> HasEnoughRaceDaysAsync(int pilotId)
    {
        return await _db.Pilots.AnyAsync(p => p.Id == pilotId && p.TotalRaceDays >= _settings.MinRaceDaysToCreate);
    }

    private async Task<bool> IsAllowedPatronAsync(int pilotId)
    {
        return _settings.AllowPatronsToCreate && await _db.PatreonSupporters.Paying().AnyAsync(s => s.PilotId == pilotId);
    }
}

public enum TeamCreationBlocker
{
    PilotNotLinked,
    AlreadyInTeam,
    NotEnoughRaceDays
}
