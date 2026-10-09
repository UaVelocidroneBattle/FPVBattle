using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Admin.Teams;

public class TeamsViewModel
{
    public List<TeamRow> Teams { get; set; } = [];
    public int MaxTeamSize { get; set; }

    /// <summary>Names of the pilots who can own a new team right now.</summary>
    public List<string> OwnerCandidates { get; set; } = [];
}

public class TeamRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OwnerPilotName { get; set; }
    public int MemberCount { get; set; }
    public int InviteCount { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class TeamDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }
    public int MaxTeamSize { get; set; }
    public List<TeamMemberRow> Members { get; set; } = [];
    public List<TeamInviteRow> Invites { get; set; } = [];

    /// <summary>Names of the pilots who can be added to the team right now.</summary>
    public List<string> MemberCandidates { get; set; } = [];
}

public class TeamMemberRow
{
    public string UserId { get; set; } = string.Empty;
    public string? PilotName { get; set; }
    public TeamRole Role { get; set; }
    public DateTime JoinedOn { get; set; }
}

public class TeamInviteRow
{
    public string UserId { get; set; } = string.Empty;
    public string? PilotName { get; set; }
    public string? InvitedBy { get; set; }
    public DateTime InvitedOn { get; set; }
}
