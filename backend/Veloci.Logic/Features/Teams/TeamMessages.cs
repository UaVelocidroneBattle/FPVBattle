using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Teams;

public static class TeamMessages
{
    public const string NameTaken = "A team with this name already exists";
    public const string TeamNotFound = "Team not found";
    public const string PilotNotFound = "Pilot not found";
    public const string PilotNotRegistered = "This pilot is not registered yet";

    public static readonly string InvalidName = $"Team name is required and can be up to {Team.NameMaxLength} characters";
    public static readonly string DescriptionTooLong = $"Description can be up to {Team.DescriptionMaxLength} characters";
}
