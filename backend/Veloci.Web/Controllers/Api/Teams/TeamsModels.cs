using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Api.Teams;

public class CreateTeamModel
{
    public required string Name { get; set; }
    public required string Tag { get; set; }
    public string? Description { get; set; }
}

public class UpdateTeamModel
{
    public required int TeamId { get; set; }
    public required string Name { get; set; }
    public required string Tag { get; set; }
    public string? Description { get; set; }
}

public class TeamReferenceModel
{
    public required int TeamId { get; set; }
}

public class TeamMemberReferenceModel
{
    public required int TeamId { get; set; }
    public required string UserId { get; set; }
}

public class SetMemberRoleModel : TeamMemberReferenceModel
{
    public required TeamRole Role { get; set; }
}

public class TeamModel
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Tag { get; set; }
    public string? Description { get; set; }
}

public class InviteModel
{
    public required string PilotName { get; set; }
    public required int TeamId { get; set; }
}

public class TeamsOverviewModel
{
    public required List<TeamSummaryModel> MyTeams { get; set; }
    public required List<TeamInvitationModel> Invitations { get; set; }

    /// <summary>0 - if no limit</summary>
    public required int MaxTeamSize { get; set; }

    /// <summary>0 - if no limit</summary>
    public required int TeamsPerUser { get; set; }
}

public class TeamSummaryModel
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int MemberCount { get; set; }
}

public class PilotSuggestionModel
{
    public required string Name { get; set; }
    public string? Country { get; set; }
}

public class TeamInvitationModel
{
    public required int TeamId { get; set; }
    public required string TeamName { get; set; }
    public required int MemberCount { get; set; }

    /// <summary>The pilot name of the inviter. Empty when the inviter has no linked pilot.</summary>
    public string? InvitedBy { get; set; }
}

public class TeamDetailsModel
{
    public required int Id { get; set; }
    public required string Name { get; set; }

    /// <summary>Empty for teams created before tags existed.</summary>
    public string? Tag { get; set; }
    public string? Description { get; set; }
    public required int MemberCount { get; set; }

    /// <summary>The current user's role. Empty when the user is not a member.</summary>
    public TeamRole? MyRole { get; set; }

    public required List<TeamMemberModel> Members { get; set; }

    /// <summary>Only filled for the owner and admins of the team.</summary>
    public required List<TeamPendingInviteModel> PendingInvites { get; set; }
}

public class TeamMemberModel
{
    public required string UserId { get; set; }
    public required TeamRole Role { get; set; }
    public required DateTime JoinedOn { get; set; }
    public string? PilotName { get; set; }
    public string? Country { get; set; }
}

public class TeamPendingInviteModel
{
    public required string UserId { get; set; }
    public string? PilotName { get; set; }
    public string? Country { get; set; }

    /// <summary>The pilot name of the inviter. Empty when the inviter has no linked pilot.</summary>
    public string? InvitedBy { get; set; }

    public required DateTime InvitedOn { get; set; }
}
