namespace Veloci.Logic.Features.Teams;

public class TeamSettings
{
    public const string SectionName = "Teams";

    /// <summary>
    /// 0 - if no limit
    /// </summary>
    public int MaxTeamSize { get; set; }
    public int MaxOwnedTeamsPerUser { get; set; } = 1;

    /// <summary>
    /// Members a team needs before it is public: listed for everyone, shown on leaderboards and pilot profiles.
    /// Smaller teams are visible to their members and invitees only. 0 - if teams are public from the start
    /// </summary>
    public int MinPublicTeamSize { get; set; } = 3;

    /// <summary>
    /// How many teams a user can be a member of. 0 - if no limit
    /// </summary>
    public int TeamsPerUser { get; set; } = 1;

    /// <summary>
    /// Tags nobody can pick, compared ignoring case.
    /// </summary>
    public List<string> BlacklistedTags { get; set; } = [];
}
