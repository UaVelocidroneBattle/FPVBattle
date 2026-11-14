using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Notifications;

public record DayStreakPotentialLose(List<Pilot> Pilots) : INotification;
