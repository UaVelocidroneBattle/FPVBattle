using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record DayStreakAchievements(List<Pilot> Pilots) : INotification;
