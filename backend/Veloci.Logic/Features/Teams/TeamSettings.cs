namespace Veloci.Logic.Features.Teams;

public class TeamSettings
{
    public const string SectionName = "Teams";

    /// <summary>
    /// 0 - if no limit
    /// </summary>
    public int MaxTeamSize { get; set; }

    /// <summary>
    /// Members a team needs before it is public: listed for everyone, shown on leaderboards and pilot profiles.
    /// Smaller teams are visible to their members and invitees only. 0 - if teams are public from the start
    /// </summary>
    public int MinPublicTeamSize { get; set; } = 3;

    /// <summary>
    /// Race days a pilot needs before their user can create a team. 0 - if no requirement
    /// </summary>
    public int MinRaceDaysToCreate { get; set; } = 10;

    /// <summary>
    /// Whether an active Patron can create a team without the required race days
    /// </summary>
    public bool AllowPatronsToCreate { get; set; } = true;

    /// <summary>
    /// Tags nobody can pick, compared ignoring case.
    /// </summary>
    public List<string> BlacklistedTags { get; set; } = [];
}
