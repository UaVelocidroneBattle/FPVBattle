using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record DayStreakPotentialLose(List<Pilot> Pilots) : INotification;
