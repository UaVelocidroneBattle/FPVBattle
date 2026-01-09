using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Achievements.Notifications;

public record DayStreakAchievements(List<Pilot> Pilots) : INotification;
