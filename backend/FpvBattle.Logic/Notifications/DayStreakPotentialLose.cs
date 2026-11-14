using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record DayStreakPotentialLose(List<Pilot> Pilots) : INotification;
