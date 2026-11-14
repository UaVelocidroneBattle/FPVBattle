using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Teams;

public static class TeamMessages
{
    public const string NameTaken = "A team with this name already exists";
    public const string TeamNotFound = "Team not found";
    public const string PilotNotFound = "Pilot not found";
    public const string PilotNotRegistered = "This pilot is not registered yet";
    public const string PilotNotLinked = "Link your Velocidrone pilot first";
    public const string AlreadyInTeam = "You are already in a team";
    public const string TeamFull = "The team is full";
    public const string InvitedToTeam = "You are invited to this team. Accept or decline the invite above";

    public static readonly string InvalidName = $"Team name is required and can be up to {Team.NameMaxLength} characters";
    public const string TagTaken = "This tag is already used by another team";
    public const string TagNotAllowed = "This tag is not allowed. Choose another one";

    public static readonly string InvalidTag =
        $"Tag is required: {Team.TagMinLength} to {Team.TagMaxLength} letters or digits";
    public static readonly string DescriptionTooLong = $"Description can be up to {Team.DescriptionMaxLength} characters";
}
