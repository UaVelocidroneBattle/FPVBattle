using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Features.Teams;

public static class TeamMemberQueries
{
    /// <summary>The global query filter that hides ended memberships, so every query sees current members only.</summary>
    public const string CurrentMembersFilter = "CurrentMembers";

    public static IQueryable<TeamMember> IncludingFormer(this IQueryable<TeamMember> query)
        => query.IgnoreQueryFilters([CurrentMembersFilter]);
}
