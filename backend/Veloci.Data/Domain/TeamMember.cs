using System.Text.Json.Serialization;

namespace Veloci.Data.Domain;

public class TeamMember
{
    public int TeamId { get; set; }
    public virtual Team Team { get; set; }
    public required string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public TeamRole Role { get; set; }
    public DateTime JoinedOn { get; set; } = DateTime.UtcNow;
}

[JsonConverter(typeof(JsonStringEnumConverter<TeamRole>))]
public enum TeamRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}
