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
    /// How many teams a user can be a member of. 0 - if no limit
    /// </summary>
    public int TeamsPerUser { get; set; } = 1;
}
