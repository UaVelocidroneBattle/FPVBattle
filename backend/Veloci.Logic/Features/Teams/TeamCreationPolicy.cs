using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Teams;

/// <summary>
/// Whether a user may create a team themselves. Site admins creating a team from the admin area skip it.
/// </summary>
public class TeamCreationPolicy
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<PatreonSupporter> _supporters;
    private readonly TeamSettings _settings;

    public TeamCreationPolicy(
        IRepository<Pilot> pilots,
        IRepository<TeamMember> members,
        IRepository<PatreonSupporter> supporters,
        IOptions<TeamSettings> settings)
    {
        _pilots = pilots;
        _members = members;
        _supporters = supporters;
        _settings = settings.Value;
    }

    public async Task<TeamCreationBlocker?> FindBlockerAsync(ApplicationUser user)
    {
        if (user.PilotId is not { } pilotId)
            return TeamCreationBlocker.PilotNotLinked;

        if (await _members.GetAll().AnyAsync(m => m.UserId == user.Id))
            return TeamCreationBlocker.AlreadyInTeam;

        if (await HasEnoughRaceDaysAsync(pilotId) || await IsAllowedPatronAsync(pilotId))
            return null;

        return TeamCreationBlocker.NotEnoughRaceDays;
    }

    private async Task<bool> HasEnoughRaceDaysAsync(int pilotId)
    {
        return await _pilots.GetAll().AnyAsync(p => p.Id == pilotId && p.TotalRaceDays >= _settings.MinRaceDaysToCreate);
    }

    private async Task<bool> IsAllowedPatronAsync(int pilotId)
    {
        return _settings.AllowPatronsToCreate && await _supporters.GetAll().Paying().AnyAsync(s => s.PilotId == pilotId);
    }
}

public enum TeamCreationBlocker
{
    PilotNotLinked,
    AlreadyInTeam,
    NotEnoughRaceDays
}
