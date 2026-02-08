using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Achievements.Notifications;

/// <summary>
/// Represents a pilot's participation in cups on a given day
/// </summary>
public record PilotCupParticipation(Pilot Pilot, List<string> CupIds);

/// <summary>
/// Notification for pilots achieving day streak milestones
/// </summary>
public record DayStreakAchievements(
    List<PilotCupParticipation> Participations
) : INotification;
