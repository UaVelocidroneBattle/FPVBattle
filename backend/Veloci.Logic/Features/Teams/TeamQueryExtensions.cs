using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Teams;

public static class TeamQueryExtensions
{
    /// <summary>Teams big enough to be shown to everyone. <paramref name="minPublicTeamSize"/> 0 keeps all teams.</summary>
    public static IQueryable<Team> Public(this IQueryable<Team> teams, int minPublicTeamSize)
        => teams.Where(t => t.Members.Count >= minPublicTeamSize);
}
