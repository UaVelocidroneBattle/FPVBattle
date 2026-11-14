using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record DayStreakPotentialLose(List<Pilot> Pilots) : INotification;
