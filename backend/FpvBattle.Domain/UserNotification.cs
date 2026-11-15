using System.Text.Json.Serialization;

namespace FpvBattle.Domain;

public class UserNotification
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public NotificationType Type { get; set; }
    public string? CupId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ReadOn { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<NotificationType>))]
public enum NotificationType
{
    General = 0,
    Achievement = 1,
    CompetitionResults = 2,
    LeagueUpdate = 3,
    TeamInvite = 4,
    TeamJoinRequest = 5,
    TeamJoinRequestApproved = 6
}
